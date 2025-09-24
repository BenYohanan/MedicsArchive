using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Service.Helpers;

public static class ServiceExtensions
{
    public static IServiceCollection RegisterHelpers(this IServiceCollection services)
    {
        services.AddScoped<IReportHelper, ReportHelper>();
        services.AddScoped<IUserHelper, UserHelper>();
        return services;
    }

    public static IServiceCollection ConfigureAppSettings(this IServiceCollection services, IConfiguration configuration)
    {
        //services.AddSingleton<IWebConfigurations>(configuration.GetSection("WebConfigurations").Get<WebConfigurations>());
        //services.AddSingleton<IEmailConfiguration>(configuration.GetSection("EmailConfiguration").Get<EmailConfiguration>());
        //services.AddSingleton<IGeneralConfiguration>(configuration.GetSection("GeneralConfiguration").Get<GeneralConfiguration>());

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

}

