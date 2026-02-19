using Data.DbContext;
using Data.Models;
using Hangfire;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Service.Helpers;
using System.Net.Http.Headers;

var builder = WebApplication.CreateBuilder(args);

// ----------------------------------------------------
// SERVICES
// ----------------------------------------------------

builder.Services.AddControllersWithViews();

builder.Services.RegisterHelpers();
builder.Services.ConfigureFormOptions();
builder.Services.ConfigureForwardedHeaders();

builder.Services.AddDbContext<AppDbContext>(options =>
	options.UseSqlServer(
		builder.Configuration.GetConnectionString("DBConnectionString")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
	options.Password.RequireDigit = false;
	options.Password.RequiredLength = 3;
	options.Password.RequiredUniqueChars = 0;
	options.Password.RequireLowercase = false;
	options.Password.RequireNonAlphanumeric = false;
	options.Password.RequireUppercase = false;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// ?? OpenAI Service with HttpClientFactory
builder.Services.AddHttpClient<IOpenAIService, OpenAIService>(client =>
{
	client.Timeout = TimeSpan.FromMinutes(3);
	client.DefaultRequestHeaders.Authorization =
		new AuthenticationHeaderValue(
			"Bearer",
			builder.Configuration["OpenAI:ApiKey"]);
});

builder.Services.AddHangfire(config =>
{
	config.UseSqlServerStorage(
		builder.Configuration.GetConnectionString("DBConnectionString"),
		new SqlServerStorageOptions
		{
			SchemaName = "hangfire",
			JobExpirationCheckInterval = TimeSpan.FromHours(1)
		});
});

builder.Services.AddHangfireServer();

var app = builder.Build();


using (var scope = app.Services.CreateScope())
{
	var serviceProvider = scope.ServiceProvider;

	try
	{
		var dbContext = serviceProvider.GetRequiredService<AppDbContext>();
		var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
		var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

		dbContext.Database.Migrate();
		CoreSeed.SeedData(roleManager, userManager);
	}
	catch (Exception ex)
	{
		Console.WriteLine($"Error seeding database: {ex.Message}");
	}
}

if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Home/Error");
	app.UseHsts();
}

app.UseForwardedHeaders();
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseHangfireConfiguration();

app.MapControllerRoute(
	name: "default",
	pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
