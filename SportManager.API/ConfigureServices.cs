using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using CloudinaryDotNet;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

namespace SportManager.API;

public static class ConfigureServices
{
    public static IServiceCollection AddServicesDependency(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddHttpContextAccessor();

        // services.AddCachingWorkspaceResolverService();
        return services;
    }

    public static IServiceCollection AddIdempotent(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        //services.AddIdempotentAPI();
        //services.AddIdempotentRedisCache(configuration);
        return services;
    }

    public static IServiceCollection AddJwtBearer(
    this IServiceCollection services,
    IConfiguration configuration,
    Action<JwtBearerOptions> configureOptions = null)
    {
        var jwtSettings = configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT Secret key not configured");

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings["Issuer"],
                ValidAudience = jwtSettings["Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
            };

            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    var accessToken = context.Request.Query["access_token"];

                    var path = context.HttpContext.Request.Path;
                    if (!string.IsNullOrEmpty(accessToken) &&
                        path.StartsWithSegments("/chatHub"))
                    {
                        context.Token = accessToken;
                    }
                    return Task.CompletedTask;
                }
            };

            configureOptions?.Invoke(options);
        });

        return services;
    }

    public static IServiceCollection AddAuthentication(
     this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment env)
    {
        if (env.EnvironmentName.Equals("IntegrationTest"))
        {
            return services;
        }

        services.AddJwtBearer(configuration, options =>
        {
            if (env.IsDevelopment())
            {
                options.RequireHttpsMetadata = false;
            }
        });

        services.AddAuthorization(options =>
        {
            // Policy mặc định
            var defaultAuthorizationPolicyBuilder =
                new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme);
            defaultAuthorizationPolicyBuilder =
                defaultAuthorizationPolicyBuilder.RequireAuthenticatedUser();
            options.DefaultPolicy = defaultAuthorizationPolicyBuilder.Build();

            // Thêm policy phân quyền
            options.AddPolicy("AdminOnly", policy =>
                policy.RequireRole("Admin"));

            options.AddPolicy("UserOnly", policy =>
                policy.RequireRole("User"));

            options.AddPolicy("ShipperOnly", policy =>
               policy.RequireRole("Shipper"));

            options.AddPolicy("AdminOrManager", policy =>
                policy.RequireRole("Admin", "Manager"));

        });

        return services;
    }

    // ====================================================================
    // THÊM EXTENSION METHOD NÀY CHO CLOUDINARY
    // ====================================================================
    public static IServiceCollection AddCloudinaryService(this IServiceCollection services, IConfiguration configuration)
    {
        // Lấy thông tin Cloudinary từ biến môi trường.
        // ASP.NET Core sẽ tự động tìm các biến có tên khớp (case-insensitive).
        // Convention là dùng `SECTION_KEY` hoặc `KEY`.
        // Ở đây, tôi dùng trực tiếp tên biến môi trường.
        var cloudName = configuration["CLOUDINARY_CLOUD_NAME"];
        var apiKey = configuration["CLOUDINARY_API_KEY"];
        var apiSecret = configuration["CLOUDINARY_API_SECRET"];

        // Kiểm tra xem các biến môi trường đã được cung cấp chưa
        if (string.IsNullOrEmpty(cloudName) || string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(apiSecret))
        {
            // Tùy chọn: Log cảnh báo nếu không tìm thấy biến môi trường.
            // Để log được ở đây, bạn cần có ILogger. Cách tốt nhất là loggerFactory
            // Hoặc đơn giản hơn, nếu bạn chắc chắn sẽ luôn cấu hình bằng biến môi trường ở Prod,
            // bạn có thể throw Exception nếu muốn bắt buộc phải có.

            // Ví dụ: Bắt buộc phải có, nếu không thì báo lỗi khởi động
            // throw new InvalidOperationException("Cloudinary environment variables (CLOUDINARY_CLOUD_NAME, CLOUDINARY_API_KEY, CLOUDINARY_API_SECRET) are not configured.");

            // Hoặc, nếu bạn muốn linh hoạt đọc từ appsettings.json nếu biến môi trường không có (ít khuyến khích hơn cho Production secrets)
            var cloudinarySection = configuration.GetSection("CloudinarySettings");
            cloudName ??= cloudinarySection["CloudName"];
            apiKey ??= cloudinarySection["ApiKey"];
            apiSecret ??= cloudinarySection["ApiSecret"];

            if (string.IsNullOrEmpty(cloudName) || string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(apiSecret))
            {
                throw new InvalidOperationException("Cloudinary configuration is missing. Please set environment variables or appsettings.json.");
            }
        }

        // Tạo đối tượng Account từ thông tin đã lấy
        Account account = new Account(cloudName, apiKey, apiSecret);

        // Đăng ký instance Cloudinary vào DI Container dưới dạng Singleton
        services.AddSingleton(new Cloudinary(account));

        return services;
    }
    // ====================================================================


    public static IServiceCollection AddCustomDapr(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        //services.AddActors(options =>
        //{
        //    //options.Actors.RegisterActor<SoStatusUpdateActor>();
        //    //options.Actors.RegisterActor<SalesOrderRequestCanceledActor>();
        //    //options.Actors.RegisterActor<PurchaseOrderCreatePackageActor>();
        //    //options.Actors.RegisterActor<PurchaseOrderActor>();
        //    //options.Actors.RegisterActor<ShipmentActor>();
        //});

        //services.AddDaprClient(
        //    opt =>
        //    {
        //        // opt.UseHttpEndpoint(configuration.GetValue<string>("Dapr:HttpEndpoint", "http://localhost:3500"));
        //        // opt.UseGrpcEndpoint(configuration.GetValue<string>("Dapr:GrpcEndpoint", "http://localhost:50001"));
        //    }
        //);

        return services;
    }

    public static WebApplication MapDaprEndpoints(this WebApplication app)
    {
        //app.MapActorsHandlers();
        //app.MapSubscribeHandler();
        return app;
    }
}