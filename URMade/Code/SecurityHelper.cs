using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using URMade.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Linq.Expressions;

namespace URMade
{
    public class SecurityHelperRequestScopeCache
    {
        public ApplicationUser LoggedInUser { get; set; }
    }

    public class SecurityHelper
    {
        private static ApplicationDbContext Context
        {
            get { return System.Web.Mvc.DependencyResolver.Current.GetService<ApplicationDbContext>(); }
        }

        private static SecurityHelperRequestScopeCache RequestCache
        {
            get { return System.Web.Mvc.DependencyResolver.Current.GetService<SecurityHelperRequestScopeCache>(); }
        }

        public static ApplicationUser GetLoggedInUser()
        {
            if (!HttpContext.Current.User.Identity.IsAuthenticated)
            {
                return null;
            }
            else if (RequestCache.LoggedInUser != null)
            {
                return RequestCache.LoggedInUser;
            }

            var claims = HttpContext.Current.Request.GetOwinContext().Authentication.User.Claims;
            var idClaim = claims.FirstOrDefault(p => p.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
            if (idClaim == null) return null;

            RequestCache.LoggedInUser = Context.Users.Find(idClaim.Value);
            return RequestCache.LoggedInUser;
        }

        /// <summary>
        /// Returns true if username exists and it is disabled.
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public static bool UsernameIsDisabled(string username)
        {
            ApplicationUser user = Context.Users.Where(p => p.UserName == username).FirstOrDefault();
            if (user == null) return false;

            return user.LoginDisabled;
        }

        //public static bool LoggedInUserCanEditProperty<TEntity>( TEntity entity, string propertyName)
        //    where TEntity : IHasPropertyPermissions
        //{
        //    ApplicationUser user = GetLoggedInUser();
        //    if (user == null) return false;

        //    PermissionGroup group = user.PermissionGroup();
        //    group |= UsersPermissionGroupsForEntity(user, entity);

        //    return entity.GroupCanAccessProperty(group, propertyName);
        //

        public static bool UserCanEditProperty<TEntity, TProperty>(
            ApplicationUser user,
            TEntity entity,
            Expression<Func<TEntity, TProperty>> property)
            where TEntity : IHasPropertyPermissions
        {
            if (user == null) return false;

            PermissionGroup group = user.PermissionGroup();
            group |= UsersPermissionGroupsForEntity(user, entity);

            string propertyName = property.PropertyName();

            return entity.GroupCanAccessProperty(group, propertyName);
        }

        public static bool LoggedInUserCanEditProperty<TEntity, TProperty>(
            TEntity entity,
            Expression<Func<TEntity, TProperty>> property)
            where TEntity : IHasPropertyPermissions
        {
            return UserCanEditProperty(GetLoggedInUser(), entity, property);
        }

        /// <summary>
        /// Check to see if the logged in user has ALL of the permissions 
        /// specified
        /// </summary>
        /// <param name="permissions"></param>
        /// <returns>True if the logged in user has ALL of the permissions 
        /// specified</returns>
        public static bool LoggedInUserHas(params Permission[] permissions)
        {
            var user = GetLoggedInUser();
            return user != null && user.HasPermission(permissions);
        }

        /// <summary>
        /// Check to see if the logged in user has ANY of the permissions specified
        /// </summary>
        /// <returns>True if the logged in user has ANY of the permissions specified
        /// specified</returns>
        public static bool LoggedInUserHasAny(params Permission[] permissions)
        {
            var user = GetLoggedInUser();
            return user != null && user.HasPermissionAny(permissions);
        }

        public static bool LoggedInUserHasPermissions<T>(T entity, params Permission[] permission)
        {
            var user = GetLoggedInUser();
            Permission grantedPermissions = UserPermissionsForEntity(user, entity);
            foreach (Permission p in permission)
            {
                if ((p & grantedPermissions) != p) return false;
            }

            return true;
        }

        public static Permission UserPermissionsForEntity<T>(ApplicationUser user, T entity)
        {
            Permission result = Permission.None;

            List<int> userGroupIds = user.UserGroups.Select(p => p.UserGroupId).ToList();
            if (userGroupIds.Count() == 0) return result;

            result |= PermissionHelper.GetPermissionForPermissionGroup(UsersPermissionGroupsForEntity(user, entity));

            var properties = entity.GetType().GetPropertiesWithAttribute<HasPermissionAttribute>();

            entity
                .GetType()
                .GetPropertiesWithAttribute<HasPermissionAttribute>()
                .ForEachReferencedUserGroup(entity, (userGroup, property) =>
                {
                    if (userGroupIds.Contains(userGroup.UserGroupId))
                    {
                        result |= property.CustomPermission();
                    }
                });

            return result;
        }

        public static PermissionGroup UsersPermissionGroupsForEntity<T>(ApplicationUser user, T entity)
        {
            PermissionGroup result = PermissionGroup.None;

            List<int> userGroupIds = user.UserGroups.Select(p => p.UserGroupId).ToList();
            if (userGroupIds.Count() == 0) return result;

            entity
                .GetType()
                .GetPropertiesWithAttribute<HasPermissionGroupAttribute>()
                .ForEachReferencedUserGroup(entity, (userGroup, property) =>
                {
                    if (userGroupIds.Contains(userGroup.UserGroupId))
                    {
                        result |= property.CustomPermissionGroup();
                    }
                });

            return result;
        }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="userId"></param>
		/// <param name="permission"></param>
		/// <returns></returns>

		public static bool IsLoggedInWithPermission(string userId, Permission permission)
		{
			var current = GetLoggedInUser();
			return current != null && current.Id == userId && current.HasPermission(permission);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="userId"></param>
		/// <param name="permission"></param>
		/// <param name="alternate"></param>
		/// <returns></returns>

		public static bool IsLoggedInWithPermission(string userId, Permission permission, Permission alternate)
		{
			var current = GetLoggedInUser();

			if (current != null)
				return (current.Id == userId && current.HasPermission(permission)) || current.HasPermission(alternate);

			return false;
		}

        public static bool TokenValid(string token, string email)
        {
            var user = Context.Users
                .SingleOrDefault(u => u.PasswordResetToken != null
                    && u.PasswordResetToken == token
                    && u.Email == email);

            if (user == null
                || !user.PasswordResetTokenExpires.HasValue
                || user.PasswordResetTokenExpires.Value < DateTime.UtcNow)
            {
                return false;
            }

            return true;
        }

        public static ApplicationUser FindUserByToken(string token, string email)
        {
            var user = Context.Users
                .SingleOrDefault(u => u.PasswordResetToken != null
                    && u.PasswordResetToken == token
                    && u.Email == email);

            if (user == null
                || !user.PasswordResetTokenExpires.HasValue
                || user.PasswordResetTokenExpires.Value < DateTime.UtcNow)
            {
                return null;
            }

            return user;
        }

        public static void UpdatePassword(string userId, string newPassword)
        {
            var userManager = new UserManager<ApplicationUser>(
                  new Microsoft.AspNet.Identity.EntityFramework
                    .UserStore<ApplicationUser>(Context));

            userManager.RemovePassword(userId);
            userManager.AddPassword(userId, newPassword);

            Context.SaveChanges();
        }
    }

    public static class PropertyInfoPermissionExtensions
    {
        public static IEnumerable<PropertyInfo> GetPropertiesWithAttribute<TAttribute>(this Type baseType)
        {
            // If we're reflecting on a PROXY for an entity class, our attributes
            // won't be found. This tests the entity class for attributes if the baseType
            // we're passed is an entity proxy.
            if (baseType.BaseType != null && baseType.Namespace == "System.Data.Entity.DynamicProxies")
            {
                baseType = baseType.BaseType;
            }

            var properties = baseType.GetProperties();
            var attrType = typeof(TAttribute);

            var propertiesWithAttr = properties.Where(p => p.IsDefined(attrType, true));

            return propertiesWithAttr;
        }

        public static void ForEachReferencedUserGroup<TEntity>(
            this IEnumerable<PropertyInfo> properties,
            TEntity entity,
            Action<UserGroup, PropertyInfo> callback)
        {
            properties
               .Where(p => p.PropertyType == typeof(UserGroup)).ToList()
               .ForEach(p =>
               {
                   var userGroup = (UserGroup)p.GetValue(entity);
                   if (userGroup != null)
                   {
                       callback(userGroup, p);
                   }
               });

            properties
                .Where(p => p.PropertyType == typeof(ICollection<UserGroup>)).ToList()
                .ForEach(p =>
                {
                    var userGroups = (ICollection<UserGroup>)p.GetValue(entity);
                    if (userGroups != null && userGroups.Count() > 0)
                    {
                        foreach (var group in userGroups.ToList())
                        {
                            callback(group, p);
                        }
                    }
                });
        }

        public static PermissionGroup CustomPermissionGroup(this PropertyInfo propertyInfo)
        {
            var attr = (HasPermissionGroupAttribute)Attribute.GetCustomAttribute(propertyInfo, typeof(HasPermissionGroupAttribute));
            if (attr != null)
            {
                return attr.PermissionGroup;
            }
            else
            {
                return PermissionGroup.None;
            }
        }

        public static Permission CustomPermission(this PropertyInfo propertyInfo)
        {
            var attr = (HasPermissionAttribute)Attribute.GetCustomAttribute(propertyInfo, typeof(HasPermissionAttribute));
            if (attr != null)
            {
                return attr.Permission;
            }
            else
            {
                return Permission.None;
            }
        }
    }

    public class HasPermissionAttribute : Attribute
    {
        public Permission Permission { get; set; }

        public HasPermissionAttribute()
        {
            Permission = Permission.None;
        }

        public HasPermissionAttribute(params Permission[] permission)
        {
            Permission = Permission.None;

            foreach (Permission p in permission)
            {
                Permission |= p;
            }
        }
    }

    public class HasPermissionGroupAttribute : Attribute
    {
        public PermissionGroup PermissionGroup { get; set; }

        public HasPermissionGroupAttribute()
        {
            PermissionGroup = PermissionGroup.None;
        }

        public HasPermissionGroupAttribute(PermissionGroup group)
        {
            PermissionGroup = group;
        }
    }
}