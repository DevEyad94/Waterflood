global using BackEndWaterFloodApp.Data;
global using BackEndWaterFloodApp.Extensions;
global using BackEndWaterFloodApp.Extensions.Pagination;
global using BackEndWaterFloodApp.Models;
global using AutoMapper;
global using Microsoft.EntityFrameworkCore;
using System.IO;
using BackEndWaterFloodApp.Constants;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

const string MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

builder.Services.AddWaterfloodInfrastructure(builder.Configuration);
builder.Services.AddWaterfloodAuthentication(builder.Configuration);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddOpenApiDocument(options =>
{
    options.DocumentName = "v1";
    options.Title = "Waterflood Performance Management API";
    options.Version = "v1";
    options.Description =
        "API for managing waterflood performance, well measurements, and analytics";
    options.ApiGroupNames = new[] { "v1" };

    options.PostProcess = document =>
    {
        document.Info = new NSwag.OpenApiInfo
        {
            Version = "v1",
            Title = "Waterflood Performance Management API",
            Description =
                "API for managing waterflood performance, well measurements, and analytics",
        };
        document.SecurityDefinitions.Add(
            "bearerAuth",
            new NSwag.OpenApiSecurityScheme
            {
                Type = NSwag.OpenApiSecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                Description = "JWT Authorization header using the Bearer scheme.",
            }
        );
        document.Security.Add(
            new NSwag.OpenApiSecurityRequirement { { "bearerAuth", Array.Empty<string>() } }
        );
    };
});

builder.Services.AddCors(options =>
{
    options.AddPolicy(
        name: MyAllowSpecificOrigins,
        policy =>
        {
            policy
                .WithOrigins("http://localhost:4200")
                .AllowAnyHeader()
                .AllowCredentials()
                .AllowAnyMethod();
        }
    );
});

Constant.SetEnvironment(builder.Environment.EnvironmentName);

var app = builder.Build();

if (app.Environment.IsDevelopment() || app.Environment.IsStaging())
{
    app.UseOpenApi(options => options.Path = "/openapi/{documentName}.json");
    app.MapScalarApiReference(options =>
    {
        options.Title = "Waterflood Performance Management API";
        options.OpenApiRoutePattern = "/openapi/{documentName}.json";
    });
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseStaticFiles(
    new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(
            Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "browser")
        ),
        RequestPath = "",
    }
);

app.UseCors(MyAllowSpecificOrigins);
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapFallbackToFile("browser/index.html");
app.Run();
