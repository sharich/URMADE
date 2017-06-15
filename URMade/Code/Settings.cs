using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using URMade.Models;
using System.Web.Mvc;
using Newtonsoft.Json;

namespace URMade
{
    public class Settings
    {
        public static string SiteName { get { return "UR Made"; } }
        public static string Domain { get { return ""; } }
        public static string FromAddress { get { return "no-reply@urmade.com"; } }

        private static ApplicationDbContext Context
        {
            get { return System.Web.Mvc.DependencyResolver.Current.GetService<ApplicationDbContext>(); }
        }

        /// <summary>
        /// If a UserGroup named "Developers" exists: 1) Any users with an email on this list will be added to it
        /// when the application starts. 2) If user accounts don't yet exist yet with any of these emails
        /// they will be created and added to the "Developers" UserGroup.
        /// </summary>
        public static string[] DeveloperAccountEmails = {
            "stephen@netguava.com",
        };

        public static bool UsernameSameAsEmail = true;
        public static bool AdminsCanSetPassword = true;

        public static string BaseUrl
        {
            get
            {
                var r = HttpContext.Current.Request;
                string url = string.Format("{0}://{1}", r.Url.Scheme, r.Url.Authority);
                if (url.Contains("localhost") == false)
                {
                    url += "/app/";
                }

                return url;
            }
        }

    }
}