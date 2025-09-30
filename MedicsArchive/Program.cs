using Data.DbContext;
using Data.Models;
using Hangfire;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.RegisterHelpers();
builder.Services.ConfigureFormOptions();
builder.Services.ConfigureForwardedHeaders();

// Register the database context
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DBConnectionString")));
builder.Services.AddHangfire(x => x.UseSqlServerStorage(builder.Configuration.GetConnectionString("DBConnectionHangFire")));

// Register Identity services
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 3;
    options.Password.RequiredUniqueChars = 0;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
}).AddEntityFrameworkStores<AppDbContext>();

var app = builder.Build();

// Seed the database
using (var scope = app.Services.CreateScope())
{
    var serviceProvider = scope.ServiceProvider;
    try
    {
        var dbContext = serviceProvider.GetRequiredService<AppDbContext>();
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        // Ensure the database is created and migrated
        dbContext.Database.Migrate();

        // Run the seed logic
        CoreSeed.SeedData(roleManager, userManager);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error occurred seeding the database: {ex.Message}");
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAuthentication();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();
app.UseHangfireConfiguration();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();
