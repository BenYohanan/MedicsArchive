using Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.ViewModels
{
	public class ApplicationUserViewModel
	{
		public string? Id { get; set; }
		public string? FullName { get; set; }
		public string? Email { get; set; }
		public string? PhoneNumber { get; set; }
		public DateTime? DateRegistered { get; set; }
		public PassWordType PassWordType { get; set; }
	}
}
