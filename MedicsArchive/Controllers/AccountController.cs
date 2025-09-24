using Data.Models;
using MedicsArchive.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Service.Helpers;
using System.Security.Claims;

namespace MedicsArchive.Controllers
{
	public class AccountController : Controller
	{
		private readonly SignInManager<ApplicationUser> _signInManager;
		private UserManager<ApplicationUser> _userManager;
		private readonly IUserHelper _userHelper;
		public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IUserHelper userHelper)
		{
			_userManager = userManager;
			_signInManager = signInManager;
			_userHelper = userHelper;
		}
		public IActionResult Login()
		{
			return View();
		}
        [HttpPost]
        public async Task<JsonResult> Login(string emailorphone, string password)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(emailorphone) || string.IsNullOrWhiteSpace(password))
                {
                    return ResponseHelper.JsonError("Please fill the form correctly");
                }

                var filterSpace = emailorphone.Replace(" ", "");

                var user = await _userHelper.FindByEmailAsync(filterSpace).ConfigureAwait(false);
                if (user == null)
                {
                    return ResponseHelper.JsonError("Invalid detail or account does not exist, contact your Admin");
                }

                var userRoles = (List<string>)await _userManager.GetRolesAsync(user).ConfigureAwait(false);

                var userPasswordType = user.PassWordType;
                if (userPasswordType != PassWordType.DoNotExpire && userRoles.Contains("user", StringComparer.OrdinalIgnoreCase))
                {
                    var creationDate = user.DateRegistered;
                    if (creationDate.HasValue)
                    {
                        if (userPasswordType == PassWordType.TwoWeeks)
                        {
                            var timeElapsed = DateTime.UtcNow - creationDate.Value;
                            if (timeElapsed.TotalHours > 336)
                                return ResponseHelper.JsonError("Password expired. Contact admin");
                        }
                        if (userPasswordType == PassWordType.OneWeek)
                        {
                            var timeElapsed = DateTime.UtcNow - creationDate.Value;
                            if (timeElapsed.TotalHours > 168)
                                return ResponseHelper.JsonError("Password expired. Contact admin");
                        }
                    }
                }

                var result = await _signInManager.PasswordSignInAsync(user, password, true, lockoutOnFailure: false)
                                                  .ConfigureAwait(false);

                if (!result.Succeeded)
                {
                    return ResponseHelper.JsonError("Invalid user name or password");
                }

                var claims = new List<Claim>
                {
                    new Claim("FullName", user.FullName ?? string.Empty)
                };
                await _signInManager.SignInWithClaimsAsync(user, isPersistent: true, claims);

                var url = _userHelper.GetValidatedUrl(userRoles);
                return Json(new { isError = false, dashboard = url });
            }
            catch (Exception)
            {
                return ResponseHelper.JsonError("An error occurred. Please try again later.");
            }
        }


        [HttpPost]
		public async Task<IActionResult> LogOut()
		{
			await _signInManager.SignOutAsync();
			return RedirectToAction("Login", "Account");
		}
	}
}
