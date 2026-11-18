using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using SmartRetail.Application.Common.Settings;
using SmartRetail.Application.Interfaces.Services;
using SmartRetail.Domain.Entities;
using SmartRetail.Infrastructure.Persistence.Contexts;
using SmartRetail.Infrastructure.Services;

namespace SmartRetail.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            // 1. DbContext & Identity
            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"))
                       .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
            });

            services.AddIdentity<User, IdentityRole>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.SignIn.RequireConfirmedEmail = true;
                options.Password.RequiredLength = 8;
                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

            // 2. Authentication & JWT
            var secretKey = configuration["JWT:SecritKey"] ?? configuration["JWT:Key"] ?? "super_secret_smart_retail_jwt_key_2026_default_long_enough_key!";
            var issuer = configuration["JWT:IssuerIP"] ?? configuration["JWT:Issuer"];
            var audience = configuration["JWT:AudienceIP"] ?? configuration["JWT:Audience"];

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = !string.IsNullOrEmpty(issuer),
                    ValidIssuer = issuer,
                    ValidateAudience = !string.IsNullOrEmpty(audience),
                    ValidAudience = audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
            });

            // 3. Settings Configurations
            services.Configure<BrevoSettings>(configuration.GetSection("BrevoSettings"));
            services.Configure<GmailSettings>(configuration.GetSection("GmailSettings"));
            services.Configure<SendGridSettings>(configuration.GetSection("SendGridSettings"));
            services.Configure<EmailSenderSettings>(configuration.GetSection("EmailSenderSettings"));

            // 4. Services Registration
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<IStoreService, StoreService>();
            services.AddScoped<IDeviceService, DeviceService>();

            var emailSenderType = configuration["EmailSenderSettings:SenderType"];
            if (emailSenderType == "SendGrid")
            {
                services.AddScoped<IEmailSender, SendEmailViaSendGridAsync>();
            }
            else if (emailSenderType == "Gmail")
            {
                services.AddScoped<IEmailSender, SendViaGmailSmtpAsync>();
            }
            else
            {
                // Brevo is the default email service
                services.AddScoped<IEmailSender, SendViaBrevoSmtpAsync>();
            }

            services.AddHostedService<UserInactivityMonitor>();

            return services;
        }
    }
}
