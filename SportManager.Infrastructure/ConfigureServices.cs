using FirebaseAdmin.Messaging;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SportManager.API.Services;
using SportManager.Application.Abstractions;
using SportManager.Application.Common.Interfaces;
using SportManager.Infrastructure.Persistence;
using SportManager.Infrastructure.Services;
using System;

namespace SportManager.Infrastructure;

public static class ConfigureServices
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Đăng ký các dịch vụ
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IAuthService, AuthService>();

        // DbContext cho thao tác ghi
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"),
            npgsqlOptionsAction: npgsqlOptions =>
            {
                npgsqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorCodesToAdd: null);
                npgsqlOptions.CommandTimeout(60);
            }));

        // DbContext cho thao tác đọc (với NoTracking)
        services.AddDbContext<ReadOnlyApplicationDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"),
            npgsqlOptionsAction: npgsqlOptions =>
            {
                npgsqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorCodesToAdd: null);
                npgsqlOptions.CommandTimeout(60);
            }));

        // Đăng ký interfaces với implementations tương ứng
        services.AddScoped<IApplicationDbContext>(provider =>
            provider.GetRequiredService<AppDbContext>());

        services.AddScoped<IReadOnlyApplicationDbContext>(provider =>
            provider.GetRequiredService<ReadOnlyApplicationDbContext>());

        var firebaseConfigJson = Environment.GetEnvironmentVariable("FIREBASE_ADMIN_SDK_CONFIG");

        if (string.IsNullOrEmpty(firebaseConfigJson))
        {
            // Hoặc đọc từ appsettings.json nếu bạn muốn fallback hoặc trong môi trường dev
            // var configuration = services.BuildServiceProvider().GetRequiredService<IConfiguration>();
            // firebaseConfigJson = configuration["Firebase:ServiceAccountKeyJson"];
            throw new InvalidOperationException("Biến môi trường 'FIREBASE_ADMIN_SDK_CONFIG' không được tìm thấy hoặc rỗng.");
        }

        FirebaseApp.Create(new AppOptions()
        {
            // Sử dụng GoogleCredential.FromJson để tạo credential từ chuỗi JSON
            Credential = GoogleCredential.FromJson(firebaseConfigJson),
        });

        services.AddSingleton(FirebaseMessaging.DefaultInstance);
        services.AddScoped<IPushNotificationService, FirebasePushNotificationService>();


        return services;
    }
}