using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web.Mvc;
using URMade.Models;

namespace URMade
{
    public class DataHelper
    {
        private static ApplicationDbContext context
        {
            get { return System.Web.Mvc.DependencyResolver.Current.GetService<ApplicationDbContext>(); }
        }
		
        private static VideoRepository VideoRepo
        {
            get { return System.Web.Mvc.DependencyResolver.Current.GetService<VideoRepository>(); }
        }

        private static SongRepository SongRepo
        {
            get { return System.Web.Mvc.DependencyResolver.Current.GetService<SongRepository>(); }
        }

        public static string PropertyName<T, TResult>(Expression<Func<T, TResult>> property)
        {
            var me = property.Body as MemberExpression;
            if (me != null)
            {
                return me.Member.Name;
            }
            return null;
        }

        public static string EncodeUrlIdFromString(string value)
        {
            char[] padding = { '=' };
            return System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(value))
                    .TrimEnd(padding).Replace('+', '-').Replace('/', '_');

        }

        public static string DecodeStringFromUrlId(string encodedId)
        {
            if (string.IsNullOrWhiteSpace(encodedId)) return "";
            string incoming = encodedId.Replace('_', '/').Replace('-', '+');
            switch (encodedId.Length % 4)
            {
                case 2: incoming += "=="; break;
                case 3: incoming += "="; break;
            }
            byte[] bytes = Convert.FromBase64String(incoming);

            return System.Text.Encoding.UTF8.GetString(bytes);
        }
		
        public static List<SelectListItem> GetSelectListItems(string OptionGroup, string FirstValue = null, string FirstText = null)
        {
            List<SelectListItem> items = (
                new SelectList(context.SelectOptions
                                    .Where(p => p.OptionGroup == OptionGroup)
                                    .OrderBy(s => s.SortOrder)
                                    .Select(p => new { value = p.Value, text = p.Text })
                    , "value", "text")
                ).ToList();
            if (FirstValue != null || FirstText != null) items.Insert(0, (new SelectListItem { Text = FirstText, Value = FirstValue }));
            return items;
        }
		
        public static List<SelectListItem> GetMySongSelectListItems(string FirstValue = null, string FirstText = null)
        {
			if(SecurityHelper.GetLoggedInUser()==null) return new List<SelectListItem>();
            List<SelectListItem> items = (
                new SelectList(SongRepo.GetUserSongs(SecurityHelper.GetLoggedInUser().Id)
                                    .Select(p => new { value = p.SongId, text = p.Title })
                    , "value", "text")
                ).ToList();
            if (FirstValue != null || FirstText != null) items.Insert(0, (new SelectListItem { Text = FirstText, Value = FirstValue }));
            return items;
        }

        public static List<SelectListItem> GetMyVideoSelectListItems(string FirstValue = null, string FirstText = null)
        {
			if(SecurityHelper.GetLoggedInUser()==null) return new List<SelectListItem>();
            List<SelectListItem> items = (
                new SelectList(VideoRepo.GetUserVideos(SecurityHelper.GetLoggedInUser().Id)
                                    .Select(p => new { value = p.VideoId, text = p.Title })
                    , "value", "text")
                ).ToList();
            if (FirstValue != null || FirstText != null) items.Insert(0, (new SelectListItem { Text = FirstText, Value = FirstValue }));
            return items;
        }

        public static IEnumerable<SelectListItem> USStates
        {
            get
            {
                #region states definition

                return new[] {
                    new SelectListItem { Value = "", Text = "Select One"},
                    new SelectListItem { Value = "AL", Text = "Alabama"},
                    new SelectListItem { Value = "AK", Text = "Alaska"},
                    new SelectListItem { Value = "AS", Text = "American Samoa"},
                    new SelectListItem { Value = "AZ", Text = "Arizona"},
                    new SelectListItem { Value = "AR", Text = "Arkansas"},
                    new SelectListItem { Value = "CA", Text = "California"},
                    new SelectListItem { Value = "CO", Text = "Colorado"},
                    new SelectListItem { Value = "CT", Text = "Connecticut"},
                    new SelectListItem { Value = "DE", Text = "Delaware"},
                    new SelectListItem { Value = "DC", Text = "District Of Columbia"},
                    new SelectListItem { Value = "FM", Text = "Federated States Of Micronesia"},
                    new SelectListItem { Value = "FL", Text = "Florida"},
                    new SelectListItem { Value = "GA", Text = "Georgia"},
                    new SelectListItem { Value = "GU", Text = "Guam"},
                    new SelectListItem { Value = "HI", Text = "Hawaii"},
                    new SelectListItem { Value = "ID", Text = "Idaho"},
                    new SelectListItem { Value = "IL", Text = "Illinois"},
                    new SelectListItem { Value = "IN", Text = "Indiana"},
                    new SelectListItem { Value = "IA", Text = "Iowa"},
                    new SelectListItem { Value = "KS", Text = "Kansas"},
                    new SelectListItem { Value = "KY", Text = "Kentucky"},
                    new SelectListItem { Value = "LA", Text = "Louisiana"},
                    new SelectListItem { Value = "ME", Text = "Maine"},
                    new SelectListItem { Value = "MH", Text = "Marshall Islands"},
                    new SelectListItem { Value = "MD", Text = "Maryland"},
                    new SelectListItem { Value = "MA", Text = "Massachusetts"},
                    new SelectListItem { Value = "MI", Text = "Michigan"},
                    new SelectListItem { Value = "MN", Text = "Minnesota"},
                    new SelectListItem { Value = "MS", Text = "Mississippi"},
                    new SelectListItem { Value = "MO", Text = "Missouri"},
                    new SelectListItem { Value = "MT", Text = "Montana"},
                    new SelectListItem { Value = "NE", Text = "Nebraska"},
                    new SelectListItem { Value = "NV", Text = "Nevada"},
                    new SelectListItem { Value = "NH", Text = "New Hampshire"},
                    new SelectListItem { Value = "NJ", Text = "New Jersey"},
                    new SelectListItem { Value = "NM", Text = "New Mexico"},
                    new SelectListItem { Value = "NY", Text = "New York"},
                    new SelectListItem { Value = "NC", Text = "North Carolina"},
                    new SelectListItem { Value = "ND", Text = "North Dakota"},
                    new SelectListItem { Value = "MP", Text = "Northern Mariana Islands"},
                    new SelectListItem { Value = "OH", Text = "Ohio"},
                    new SelectListItem { Value = "OK", Text = "Oklahoma"},
                    new SelectListItem { Value = "OR", Text = "Oregon"},
                    new SelectListItem { Value = "PW", Text = "Palau"},
                    new SelectListItem { Value = "PA", Text = "Pennsylvania"},
                    new SelectListItem { Value = "PR", Text = "Puerto Rico"},
                    new SelectListItem { Value = "RI", Text = "Rhode Island"},
                    new SelectListItem { Value = "SC", Text = "South Carolina"},
                    new SelectListItem { Value = "SD", Text = "South Dakota"},
                    new SelectListItem { Value = "TN", Text = "Tennessee"},
                    new SelectListItem { Value = "TX", Text = "Texas"},
                    new SelectListItem { Value = "UT", Text = "Utah"},
                    new SelectListItem { Value = "VT", Text = "Vermont"},
                    new SelectListItem { Value = "VI", Text = "Virgin Islands"},
                    new SelectListItem { Value = "VA", Text = "Virginia"},
                    new SelectListItem { Value = "WA", Text = "Washington"},
                    new SelectListItem { Value = "WV", Text = "West Virginia"},
                    new SelectListItem { Value = "WI", Text = "Wisconsin"},
                    new SelectListItem { Value = "WY", Text = "Wyoming"}
                };
                #endregion
            }
        }

        public static IEnumerable<SelectListItem> EnumAsOptions(
            Type EnumType,
            string FirstItemText = "Select One")
        {
            List<SelectListItem> items = new List<SelectListItem>();

            items.Add(new SelectListItem { Value = "", Text = FirstItemText });
            foreach (Enum value in EnumType.GetEnumValues())
            {
                items.Add(new SelectListItem { Value = value.ToString(), Text = value.GetName() });
            }

            return items;
        }
    }

    public static class DataHelperExtensions
    {
        public static string PropertyName<T, TResult>(this Expression<Func<T, TResult>> property)
        {
            var me = property.Body as MemberExpression;
            if (me != null)
            {
                return me.Member.Name;
            }
            return null;
        }
    }
    

    public static class StringExtensions
    {

        public static string SplitCamelCase(this string str)
        {
            if (string.IsNullOrWhiteSpace(str)) return str;
            return System.Text.RegularExpressions.Regex.Replace(
                System.Text.RegularExpressions.Regex.Replace(
                    str,
                    @"(\P{Ll})(\P{Ll}\p{Ll})",
                    "$1 $2"
                ),
                @"(\p{Ll})(\P{Ll})",
                "$1 $2"
            );
        }

        public static string Right(this string sValue, int iMaxLength)
        {
            //Check if the value is valid
            if (string.IsNullOrEmpty(sValue))
            {
                //Set valid empty string as string could be null
                sValue = string.Empty;
            }
            else if (sValue.Length > iMaxLength)
            {
                //Make the string no longer than the max length
                sValue = sValue.Substring(sValue.Length - iMaxLength, iMaxLength);
            }

            //Return the string
            return sValue;
        }
        public static string Left(this string sValue, int iMaxLength)
        {
            //Check if the value is valid
            if (string.IsNullOrEmpty(sValue))
            {
                //Set valid empty string as string could be null
                sValue = string.Empty;
            }
            else if (sValue.Length > iMaxLength)
            {
                //Make the string no longer than the max length
                sValue = sValue.Substring(0, iMaxLength);
            }

            //Return the string
            return sValue;
        }
    }
}