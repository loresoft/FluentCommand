using System;
using System.Security.Principal;
using System.Web;
using System.Web.Hosting;

namespace FluentCommand.Security
{
    public static class UserHelper
    {
        private static string GetCurrentUserName()
        {
            if (!HostingEnvironment.IsHosted)
                return Environment.UserName;

            IPrincipal currentUser = null;
            HttpContext current = HttpContext.Current;
            if (current != null)
                currentUser = current.User;

            if ((currentUser != null))
                return currentUser.Identity.Name;

            return Environment.UserName;
        }

        public static string Current(bool includeDomain = false)
        {
            string username = GetCurrentUserName();
            if (!includeDomain)
                return username;

            string name = username;

            var parts = username.Split('\\');
            if (parts.Length == 2)
                name = parts[1];

            return name;
        }

    }
}
