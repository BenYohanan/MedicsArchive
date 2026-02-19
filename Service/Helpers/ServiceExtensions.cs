using Hangfire;
using Hangfire.Dashboard;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.DependencyInjection;
using Service.Helpers;

public static class ServiceExtensions
{
	public static IServiceCollection RegisterHelpers(this IServiceCollection services)
	{
		services.AddScoped<IReportHelper, ReportHelper>();
		services.AddScoped<IUserHelper, UserHelper>();
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
			options.ForwardedHeaders =
				ForwardedHeaders.XForwardedFor |
				ForwardedHeaders.XForwardedProto;

			options.KnownNetworks.Clear();
			options.KnownProxies.Clear();
		});

		return services;
	}

	public static IApplicationBuilder UseHangfireConfiguration(this IApplicationBuilder app)
	{
		app.UseHangfireDashboard("/MedImageHangFire", new DashboardOptions
		{
			Authorization = [new HangfireAuthorizationFilter()]
		});

		return app;
	}
}
