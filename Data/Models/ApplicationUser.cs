using Microsoft.AspNetCore.Identity;
using System.ComponentModel;

namespace Data.Models
{
	public class ApplicationUser : IdentityUser
	{
		public ApplicationUser()
		{
			IsActive = true;
			DateRegistered = DateTime.Now;
		}
		public string? FullName { get; set; }
		public DateTime? DateRegistered { get; set; }
		public bool IsActive { get; set; }
		public PassWordType PassWordType { get; set; }
	}

	public enum PassWordType
	{
        [Description("Two Weeks")]
        TwoWeeks,
        [Description("One Weeks")]
        OneWeek,
        [Description("Life Time")]
        DoNotExpire
	}
}
