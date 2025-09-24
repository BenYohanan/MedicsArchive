using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
		TwoWeeks,
		OneWeek,
		DoNotExpire
	}
}
