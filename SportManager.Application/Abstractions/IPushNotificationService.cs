namespace SportManager.Application.Abstractions;

public interface IPushNotificationService
{
    Task SendNotificationToUserAsync(string userId, string title, string body, Dictionary<string, string>? data = null);
    Task SendNotificationToDeviceAsync(string fcmToken, string title, string body, Dictionary<string, string>? data = null);
}