using Application.Common.Caching;
using Application.Helpers;
using Application.Interfaces;
using Domain.Repositories;
using Infrastructure.Authorization;
using Infrastructure.Caching;
using Infrastructure.Data;
using Infrastructure.Helper;
using Infrastructure.Identity;
using Infrastructure.Options;
using Infrastructure.RealTime;
using Infrastructure.Repositories;
using Infrastructure.Services;
using Infrastructure.Telephony;
using Infrastructure.Telephony.Options;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using System;
using System.IO;

namespace Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<OutboxState>();
            services.AddSingleton<OutboxTransactionInterceptor>();

            services.AddHttpContextAccessor();

            services.AddDbContext<AppDbContext>((provider, options) =>
            {
                var interceptor = provider.GetRequiredService<OutboxTransactionInterceptor>();
                options.UseNpgsql(
                    configuration.GetConnectionString("DefaultConnection"),
                    b =>
                    {
                        b.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName);
                        b.UseVector();
                    })
                .AddInterceptors(interceptor);
            });

            services.AddIdentity<AppUser, AppRole>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequiredLength = 8;
                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders()
            .AddClaimsPrincipalFactory<CustomClaimsPrincipalFactory>();

            services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Auth/Login";
                options.LogoutPath = "/Auth/Logout";
                options.AccessDeniedPath = "/Auth/AccessDenied";
            });

            var redisConnectionString = configuration.GetConnectionString("Redis");
            if (!string.IsNullOrWhiteSpace(redisConnectionString))
            {
                services.AddStackExchangeRedisCache(options => { options.Configuration = redisConnectionString; });
                services.AddSingleton<IConnectionMultiplexer>(sp => ConnectionMultiplexer.Connect(redisConnectionString));
                services.AddSingleton(sp => new Lazy<IConnectionMultiplexer>(() => sp.GetRequiredService<IConnectionMultiplexer>()));
            }
            else
            {
                services.AddDistributedMemoryCache();
            }

            services.AddSingleton<ICacheService, RedisCacheService>();

            services.AddSingleton<TimeProvider, NepaliTimeProvider>();

            services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<AppDbContext>());

            services.AddScoped<IRolePermissionRepository, RolePermissionRepository>();
            services.AddScoped<ITenantSecurityPolicyRepository, TenantSecurityPolicyRepository>();
            services.AddScoped<ITenantRepository, TenantRepository>();
            services.AddScoped<IApiConsentRequestRepository, ApiConsentRequestRepository>();
            services.AddScoped<ICitizenProfileRepository, CitizenProfileRepository>();
            services.AddScoped<IDocumentFileRepository, DocumentFileRepository>();
            services.AddScoped<IDocumentTemplateRepository, DocumentTemplateRepository>();
            services.AddScoped<IMissingPersonRepository, MissingPersonRepository>();
            services.AddScoped<ISifarisApplicationRepository, SifarisApplicationRepository>();
            services.AddScoped<ISifarisRepository, SifarisRepository>();
            services.AddScoped<IAlertCampaignRepository, AlertCampaignRepository>();
            services.AddScoped<ICampaignDispatchRepository, CampaignDispatchRepository>();
            services.AddScoped<IGunasoRepository, GunasoRepository>();
            services.AddScoped<IDevelopmentPlanRepository, DevelopmentPlanRepository>();

            services.AddScoped<ICurrentUserService, CurrentUserService>();
            services.AddScoped<ITenantService, TenantService>();
            services.AddScoped<IRoleService, RoleService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IFileHelper, FileHelper>();
            services.AddScoped<IDocumentForensicService, DocumentForensicService>();
            services.AddScoped<IOcrService, OcrService>();

            services.Configure<TingTingOptions>(configuration.GetSection(TingTingOptions.SectionName));
            services.AddHttpClient<ITelephonyProvider, TingTingProviderService>();
            services.AddHttpClient<IOtpService, OtpService>();

            services.Configure<LLMStudioSettings>(configuration.GetSection(LLMStudioSettings.SectionName));
            services.AddHttpClient<IAIService, LLMStudioAIService>();

            string modelDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "models");
            if (!Directory.Exists(modelDir)) Directory.CreateDirectory(modelDir);

            services.AddSingleton(sp => new Lazy<FaceRecognitionDotNet.FaceRecognition>(() =>
            {
                try { return FaceRecognitionDotNet.FaceRecognition.Create(modelDir); }
                catch (Exception ex) { throw new Exception($"Failed to initialize FaceRecognition engine. Make sure dlib models are in {modelDir}", ex); }
            }));

            services.AddSingleton(sp => new Lazy<Microsoft.ML.OnnxRuntime.InferenceSession>(() =>
            {
                var onnxPath = Path.Combine(modelDir, "w600k_r50.onnx");
                if (!File.Exists(onnxPath)) throw new FileNotFoundException("ArcFace ONNX model not found.", onnxPath);
                return new Microsoft.ML.OnnxRuntime.InferenceSession(onnxPath);
            }));

            services.AddScoped<IFaceRecognitionService, FaceRecognitionService>();

            services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
            services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();

            services.AddSignalR(hubOptions =>
            {
                hubOptions.EnableDetailedErrors = true;
                hubOptions.MaximumReceiveMessageSize = 1024 * 1024; 
            });
            
            services.AddScoped<ILiveMissingPersonNotifier, LiveMissingPersonNotifier>();

            services.AddHostedService<OutboxBackgroundService>();
            services.AddHostedService<OutboxPurgeBackgroundService>();

            return services;
        }
    }
}
