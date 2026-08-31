using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using BackEndWaterFloodApp.Application.Interfaces;
using BackEndWaterFloodApp.Application.Validators;
using BackEndWaterFloodApp.AutoMapper;
using BackEndWaterFloodApp.Constants;
using BackEndWaterFloodApp.Converters;
using BackEndWaterFloodApp.Data;
using BackEndWaterFloodApp.Infrastructure.Repositories;
using BackEndWaterFloodApp.Services.DatabaseService;
using BackEndWaterFloodApp.Services.Monitoring;
using BackEndWaterFloodApp.Services.Relationships;
using BackEndWaterFloodApp.Services.UserService;
using BackEndWaterFloodApp.Services.WaterfloodAnalytics;
using BackEndWaterFloodApp.Services.WaterfloodData;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ZSK.Infrastructure.Repositories;
using ZSK.Services.ReferenceData.Interfaces;
using ZSK.Services.ReferenceData.Services;

namespace BackEndWaterFloodApp.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddWaterfloodInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddDbContext<WaterfloodDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"))
        );

        services.AddMemoryCache();
        services.AddHttpContextAccessor();

        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IDatabaseService, DatabaseService>();

        services.AddScoped<IWaterfloodRepository, WaterfloodRepository>();
        services.AddScoped<IRelationshipRepository, RelationshipRepository>();
        services.AddScoped<IThresholdRepository, ThresholdRepository>();
        services.AddScoped<IZskReferenceRepository, ZskReferenceRepository>();

        services.AddScoped<IZskReferenceService, ZskReferenceService>();
        services.AddScoped<IWaterfloodDataService, WaterfloodDataService>();
        services.AddScoped<IWaterfloodAnalyticsService, WaterfloodAnalyticsService>();
        services.AddScoped<IMonitoringService, MonitoringService>();
        services.AddScoped<IRelationshipService, RelationshipService>();

        services.AddValidatorsFromAssemblyContaining<CreateWaterfloodRecordValidator>();

        services.AddAutoMapper(config => config.AddProfile<WaterfloodMappingProfile>());

        services
            .AddControllers()
            .AddJsonOptions(x =>
            {
                x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                x.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                x.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                x.JsonSerializerOptions.Converters.Add(new DateTimeConverter());
                x.JsonSerializerOptions.Converters.Add(new DateTimeNonNullableConverter());
            });

        services.AddSingleton<AdminOnlyConverterFactory>(provider =>
            new AdminOnlyConverterFactory(provider.GetRequiredService<IHttpContextAccessor>())
        );

        return services;
    }

    public static IServiceCollection AddWaterfloodAuthentication(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidIssuer = configuration["AppSettings:Issuer"],
                    ValidAudience = configuration["AppSettings:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(
                            configuration["AppSettings:Token"]
                                ?? throw new InvalidOperationException("JWT Token is not configured")
                        )
                    ),
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = false,
                    ValidateIssuerSigningKey = true,
                };
            });

        services.AddAuthorization(options =>
        {
            options.AddPolicy(Policies.AdminPolicy, policy => policy.RequireRole("Admin"));
            options.AddPolicy(
                Policies.EngineerPolicy,
                policy => policy.RequireRole("PetroleumEngineer")
            );
            options.AddPolicy(
                Policies.PetroleumEngineerPolicy,
                policy => policy.RequireRole("PetroleumEngineer")
            );
            options.AddPolicy(Policies.OperatorPolicy, policy => policy.RequireRole("Operator"));
            options.AddPolicy(
                Policies.AdminEngineerPolicy,
                policy => policy.RequireRole("Admin", "PetroleumEngineer")
            );
            options.AddPolicy(
                Policies.AdminOperatorPolicy,
                policy => policy.RequireRole("Admin", "Operator", "PetroleumEngineer")
            );
        });

        return services;
    }
}
