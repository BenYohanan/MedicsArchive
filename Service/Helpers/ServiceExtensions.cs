using Hangfire;
using Hangfire.Dashboard;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Service.Helpers;
using System.Security.Claims;

public static class ServiceExtensions
{
    public static IServiceCollection RegisterHelpers(this IServiceCollection services)
    {
        services.AddScoped<IReportHelper, ReportHelper>();
        services.AddScoped<IUserHelper, UserHelper>();
        services.AddScoped<IOpenAIService, OpenAIService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IEmailTemplateService, EmailTemplateService>();
        return services;
    }

    public static IServiceCollection ConfigureFormOptions(this IServiceCollection services)
    {
        services.Configure<FormOptions>(x =>
        {
            x.ValueLengthLimit = int.MaxValue;
            x.MultipartBodyLengthLimit = int.MaxValue;
            x.MultipartHeadersLengthLimit = int.MaxValue;
        });

        return services;
    }

    public static IServiceCollection ConfigureForwardedHeaders(this IServiceCollection services)
    {
        services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
        });

        return services;
    }
    public static IApplicationBuilder UseHangfireConfiguration(this IApplicationBuilder app)
    {
        var options = new BackgroundJobServerOptions
        {
            ServerName = $"{Environment.MachineName}.{Guid.NewGuid()}"
        };

        app.UseHangfireServer(options);

        var robotStorage = new SqlServerStorage(app.ApplicationServices.GetService<IConfiguration>().GetConnectionString("DBConnectionString"), new SqlServerStorageOptions
        {
            SchemaName = "hangfire",
            JobExpirationCheckInterval = TimeSpan.FromHours(1)
        });
        JobStorage.Current = robotStorage;
        app.UseHangfireDashboard("/MedImageHangFire", storage: robotStorage);

        return app;
    }

}

