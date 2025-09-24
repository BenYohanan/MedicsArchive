using Data.DbContext;
using Data.Models;
using Data.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Service.Helpers
{
	public interface IUserHelper
	{
		Task<ApplicationUser?> FindByEmailAsync(string email);
		List<ApplicationUserViewModel> GetUsers();
		string GetValidatedUrl(List<string> roles);
		Task<ApplicationUser?> RegisterUser(ApplicationUserViewModel applicationUserViewModel);
	}

	public class UserHelper : IUserHelper
	{
		private readonly AppDbContext db;
		private UserManager<ApplicationUser> _userManager;
		public UserHelper(AppDbContext db, UserManager<ApplicationUser> userManager)
		{
			_userManager = userManager;
			this.db = db;
		}
		public async Task<ApplicationUser?> FindByEmailAsync(string email)
		{
			return await db.ApplicationUsers
				.Where(s => s.Email == email && s.IsActive)
				.FirstOrDefaultAsync().ConfigureAwait(false);
		}
		public string GetValidatedUrl(List<string> roles)
		{
			var roleUrlMap = new Dictionary<string, string>
			{
				{ SeedItems.SuperAdminRole, SeedItems.SuperAdminDashboard },
				{ SeedItems.AdminRole, SeedItems.AdminDashboard },
				{ SeedItems.UserRole, SeedItems.UserDashboard }
			};

			foreach (var role in roles)
			{
				if (roleUrlMap.TryGetValue(role, out var url))
				{
					return url;
				}
			}

			return "/Account/Login";

		}
		public async Task<ApplicationUser?> RegisterUser(ApplicationUserViewModel applicationUserViewModel)
		{
			var user = new ApplicationUser
			{
				FullName = applicationUserViewModel.FullName,
				Email = applicationUserViewModel.Email,
				PhoneNumber = applicationUserViewModel.PhoneNumber,
				UserName = applicationUserViewModel.Email,
				PassWordType = applicationUserViewModel.PassWordType
			};
			var addedUser =await _userManager.CreateAsync(user, "11111").ConfigureAwait(false);
			if (addedUser.Succeeded)
			{
				var addedUserToRole = await _userManager.AddToRoleAsync(user, SeedItems.UserRole).ConfigureAwait(false);
				if (addedUserToRole.Succeeded)
				{
					return user;
				}
			}
			return null;
		}
		public List<ApplicationUserViewModel> GetUsers()
		{
			var users = _userManager.GetUsersInRoleAsync(SeedItems.UserRole).Result;
			return users.Select(r => new ApplicationUserViewModel
			{
				FullName = r.FullName,
				Email = r.Email,
				PhoneNumber = r.PhoneNumber,
				DateRegistered = r.DateRegistered.Value,
				PassWordType = r.PassWordType,
				Id = r.Id,
			}).ToList();
		}
	}
}