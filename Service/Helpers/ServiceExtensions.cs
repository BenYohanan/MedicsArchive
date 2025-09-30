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
    public class MyAuthorizationFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context)
        {
            var user = context.GetHttpContext().User;
            return user != null && user.Identity.IsAuthenticated && user.Claims.Any(c => c.Type == ClaimTypes.Role && c.Value == "SuperAdmin");
        }
    }

    public static IApplicationBuilder UseHangfireConfiguration(this IApplicationBuilder app)
    {
        var options = new BackgroundJobServerOptions
        {
            ServerName = $"{Environment.MachineName}.{Guid.NewGuid()}"
        };

        app.UseHangfireServer(options);

        var robotStorage = new SqlServerStorage(app.ApplicationServices.GetService<IConfiguration>().GetConnectionString("DBConnectionHangFire"));
        JobStorage.Current = robotStorage;

        var dashboardOptions = new DashboardOptions
        {
            Authorization = new[] { new MyAuthorizationFilter() }
        };
        app.UseHangfireDashboard("/MedicsHangFire", dashboardOptions, robotStorage);

        return app;
    }

}

