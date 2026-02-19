using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Helpers
{
	using Hangfire.Dashboard;

	public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
	{
		public bool Authorize(DashboardContext context)
		{
			var httpContext = context.GetHttpContext();

			//return httpContext.User.Identity?.IsAuthenticated == true;

			return httpContext.User.IsInRole("Admin");
		}
	}

}
