using Data.Models;
using Microsoft.AspNetCore.Identity;

namespace Data.DbContext
{
	public static class CoreSeed
	{
		public static void SeedData(RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager)
		{
			SeedRoles(roleManager).Wait();
			SeedUsers(userManager).Wait();
		}

		private static async Task SeedRoles(RoleManager<IdentityRole> roleManager)
		{
			foreach (var role in SeedItems.DefaultRoles())
			{
				if (!await roleManager.RoleExistsAsync(role.Name))
				{
					await roleManager.CreateAsync(role);
				}
			}
		}

		private static async Task SeedUsers(UserManager<ApplicationUser> userManager)
		{
			foreach (var user in SeedItems.DefaultUsers())
			{
				var existingUser = await userManager.FindByEmailAsync(user.Email);
				if (existingUser == null)
				{
					await userManager.CreateAsync(user, "11111");
					await userManager.AddToRoleAsync(user, user.UserName.ToUpper());
				}
			}
		}
	}
	public static class SeedItems
	{
		public static IList<IdentityRole> DefaultRoles()
		{
			return new List<IdentityRole>
			{
				new IdentityRole { ConcurrencyStamp = "5002C0030007800380036002C00300078", Name = SuperAdminRole, NormalizedName = "SUPERADMIN"},
				new IdentityRole {ConcurrencyStamp = "14f4bf73-9b1a-415f-9b47-626ca87f6c0e", Name = AdminRole, NormalizedName = "ADMIN"},
				new IdentityRole {ConcurrencyStamp = "0DB45C30-2FEE-47C6-AF34-7849A62B8856", Name = UserRole, NormalizedName = "USER"}
			};
		}

		public static string SuperAdminId = "7B0030007800640033006600640035003000";
		public static string SystemUserId = "30007800360034003800300030003";

		public static string AdminRole = "Admin";
		public static string SuperAdminRole = "SuperAdmin";
		public static string UserRole = "User";
		public static string SuperAdminDashboard = "/SuperAdmin/Home";
		public static string AdminDashboard = "/Report/Index";
		public static string UserDashboard = "/Report/Index";

		public static IList<ApplicationUser> DefaultUsers()
		{
			return new List<ApplicationUser>
			{
				new ApplicationUser
				{
					Id = SuperAdminId,
					ConcurrencyStamp = "030007800390066002C0030007800300037",
					Email = "superadmin@databank.com",
					IsActive = true,
					NormalizedEmail = "SUPERADMIN@DATABANK.COM",
					PasswordHash = "AQAAAAEAACcQAAAAEO3NQwqwWgetIJ/tyYRIrobEpEcvQ47xoczshXUgLyKKSuanh+CiKz//sKDMCq+PCA==",//11111
					NormalizedUserName = "SUPERADMIN",
					PhoneNumber = "0000 000 0000",
					PhoneNumberConfirmed = false,
					DateRegistered = DateTime.Now,
					UserName = "SuperAdmin"
				},

				new ApplicationUser
				{
					Id = SystemUserId,
					ConcurrencyStamp = "15000660023007080030007800670034",
					Email = "admin@databank.com",
					IsActive = true,
					PasswordHash = "AQAAAAEAACcQAAAAEO3NQwqwWgetIJ/tyYRIrobEpEcvQ47xoczshXUgLyKKSuanh+CiKz//sKDMCq+PCA==",//11111
					NormalizedEmail = "ADMIN@DATABANK.COM",
					NormalizedUserName = "ADMIN",
					PhoneNumber = "0000 000 0000",
					PhoneNumberConfirmed = false,
					DateRegistered = DateTime.Now,
					UserName = "Admin"
				}
			};
		}
	}
	
}
