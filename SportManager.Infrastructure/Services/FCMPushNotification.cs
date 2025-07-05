using FirebaseAdmin.Messaging;
using Microsoft.EntityFrameworkCore;
using SportManager.Application.Abstractions;
using SportManager.Application.Common.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System;
using Microsoft.Extensions.Logging;

namespace SportManager.Infrastructure.Services;

public class FirebasePushNotificationService : IPushNotificationService
{
    private readonly FirebaseMessaging _firebaseMessaging;
    private readonly IApplicationDbContext _dbContext;
    private readonly ILogger<FirebasePushNotificationService> _logger;

    public FirebasePushNotificationService(
        FirebaseMessaging firebaseMessaging,
        IApplicationDbContext dbContext,
        ILogger<FirebasePushNotificationService> logger)
    {
        _firebaseMessaging = firebaseMessaging;
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task SendNotificationToUserAsync(
      string userId,
      string title,
      string body,
      Dictionary<string, string>? data = null)
    {
        // Internal CancellationToken for the async operations within this method
        CancellationToken cancellationToken = CancellationToken.None;

        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
        {
            _logger.LogWarning("Invalid userId provided: {UserId}", userId);
            return; // Interface doesn't allow returning a result, so log and return
        }

        if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(body))
        {
            _logger.LogWarning("Title or body is empty for user {UserId}", userId);
            return; // Interface doesn't allow returning a result, so log and return
        }

        try
        {
            var user = await _dbContext.Users
                .Where(u => u.Id == userGuid)
                .Select(u => new { u.Id, u.FcmTokens })
                .FirstOrDefaultAsync(cancellationToken);

            if (user == null)
            {
                _logger.LogWarning("User {UserId} not found", userId);
                return;
            }

            var fcmTokens = user.FcmTokens?.Where(t => !string.IsNullOrWhiteSpace(t)).ToList()
                               ?? new List<string>();

            if (!fcmTokens.Any())
            {
                _logger.LogInformation("User {UserId} has no valid FCM tokens", userId);
                return;
            }

            _logger.LogInformation("Sending notification to user {UserId} with {TokenCount} FCM tokens",
                userId, fcmTokens.Count);

            var semaphore = new SemaphoreSlim(Environment.ProcessorCount, Environment.ProcessorCount);
            var sendTasks = fcmTokens.Select(async token =>
            {
                await semaphore.WaitAsync(cancellationToken);
                try
                {
                    // This internal call still gets a result for logging/token removal
                    return await SendToSingleTokenAsync(token, title, body, data, cancellationToken);
                }
                finally
                {
                    semaphore.Release();
                }
            });

            var results = await Task.WhenAll(sendTasks);

            var invalidTokens = results
                .Where(r => !r.Success && r.ShouldRemoveToken)
                .Select(r => r.Token)
                .ToList();

            var successCount = results.Count(r => r.Success);

            if (invalidTokens.Any())
            {
                await RemoveInvalidTokensAsync(userGuid, invalidTokens, cancellationToken);
            }

            _logger.LogInformation(
                "Notification sent to user {UserId}: {SuccessCount}/{TotalCount} successful, {InvalidCount} invalid tokens removed",
                userId, successCount, fcmTokens.Count, invalidTokens.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending notification to user {UserId}", userId);
        }
    }

    private async Task<TokenSendResult> SendToSingleTokenAsync(
        string token,
        string title,
        string body,
        Dictionary<string, string>? data,
        CancellationToken cancellationToken)
    {
        var message = new FirebaseAdmin.Messaging.Message()
        {
            Notification = new Notification
            {
                Title = title,
                Body = body
            },
            Data = data,
            Token = token,
        };

        try
        {
            string response = await _firebaseMessaging.SendAsync(message, cancellationToken);
            _logger.LogDebug("Successfully sent notification to token {Token}: {Response}",
                MaskToken(token), response);

            return new TokenSendResult
            {
                Success = true,
                Token = token,
                Response = response
            };
        }
        catch (FirebaseMessagingException ex)
        {
            bool shouldRemove = ex.MessagingErrorCode == MessagingErrorCode.Unregistered ||
                                ex.MessagingErrorCode == MessagingErrorCode.InvalidArgument;

            if (shouldRemove)
            {
                _logger.LogWarning("Invalid FCM token {Token}: {Error}",
                    MaskToken(token), ex.MessagingErrorCode);
            }
            else
            {
                _logger.LogError(ex, "Firebase messaging error for token {Token}", MaskToken(token));
            }

            return new TokenSendResult
            {
                Success = false,
                Token = token,
                ShouldRemoveToken = shouldRemove,
                ErrorMessage = ex.Message
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error sending to token {Token}", MaskToken(token));
            return new TokenSendResult
            {
                Success = false,
                Token = token,
                ErrorMessage = ex.Message
            };
        }
    }

    private async Task RemoveInvalidTokensAsync(
        Guid userId,
        List<string> invalidTokens,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Removing {Count} invalid tokens for user {UserId}",
                invalidTokens.Count, userId);

            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

            if (user != null && user.FcmTokens != null)
            {
                var currentTokens = new List<string>(user.FcmTokens);
                var updatedTokens = currentTokens.Except(invalidTokens).ToArray();

                user.FcmTokens = updatedTokens;
                await _dbContext.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Successfully removed invalid tokens for user {UserId}", userId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing invalid tokens for user {UserId}", userId);
        }
    }

    private static string MaskToken(string token)
    {
        return token.Length > 8 ? $"{token[..4]}...{token[^4..]}" : "***";
    }

    public class NotificationResult
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public string? Message { get; set; }
        public int TotalTokens { get; set; }
        public int SuccessfulSends { get; set; }
        public int InvalidTokensRemoved { get; set; }
    }

    public class TokenSendResult
    {
        public bool Success { get; set; }
        public string Token { get; set; } = string.Empty;
        public bool ShouldRemoveToken { get; set; }
        public string? Response { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public async Task SendNotificationToDeviceAsync(
        string fcmToken,
        string title,
        string body,
        Dictionary<string, string>? data = null)
    {
        // Internal CancellationToken for the async operations within this method
        CancellationToken cancellationToken = CancellationToken.None;

        try
        {
            var message = new FirebaseAdmin.Messaging.Message()
            {
                Notification = new Notification
                {
                    Title = title,
                    Body = body
                },
                Data = data,
                Token = fcmToken,
            };

            string response = await _firebaseMessaging.SendAsync(message, cancellationToken);
            _logger.LogInformation("Successfully sent notification to device with token {MaskedToken}: {Response}", MaskToken(fcmToken), response);
        }
        catch (FirebaseMessagingException ex)
        {
            // We can't return TokenSendResult, so we just log the outcome
            if (ex.MessagingErrorCode == MessagingErrorCode.Unregistered || ex.MessagingErrorCode == MessagingErrorCode.InvalidArgument)
            {
                _logger.LogWarning("FCM Token {MaskedToken} is invalid or unregistered. Consider removing it from storage for user.", MaskToken(fcmToken), ex);
            }
            else
            {
                _logger.LogError(ex, "Firebase Messaging error when sending to token {MaskedToken}: {ErrorMessage}", MaskToken(fcmToken), ex.Message);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error sending notification to device with token {MaskedToken}: {ErrorMessage}", MaskToken(fcmToken), ex.Message);
        }
    }
}