using URMade.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Reflection;
using System.Linq.Expressions;

namespace URMade
{
	/// <summary>
	/// Permission's should be used throughout the site to determine if something is allowed or not
	/// PermissionGroups used in this file only and sould be mapped to Permissions 
	/// UserGroups are user roles that are setup using the web interface to create collection's of PermissionGroups
	/// Users are then added to UserGroups
	/// </summary>

	// The system is currently broken. PermissionGroups are being referenced throughout the site which makes it very difficult to tell what permissions a specific PermissionGroup actually has access to.
	// Another problem is that there seems to be a bug in the way permissionGroups are being checked in the Security helper GroupCanAccessProperty, If you have more than one permission group, you loose access to the fields you are allowed to edit on the Vehicle edit screen.

	// As it is now, you can not give a user additional access without recompining and publishing the entire application. You should only need to recompile if you are adding functionality or granularity to the permission groups that are available to be applied using the web interface.

	[Flags]
	public enum PermissionGroup
	{
		None				= 0,
		EditMyUser			= 1 << 1,
		EditUsers			= 1 << 2,
		Impersonate			= 1 << 3,
		EditUserGroups		= 1 << 4,
		EditSelectOptions	= 1 << 5,
		EditMyArtists		= 1 << 6,
		EditArtists			= 1 << 7,
		EditContests		= 1 << 8,
		MultipleArtists		= 1 << 9,
		PremiumArtist		= 1 << 10,
		CanFavoriteAndVote	= 1 << 11
	}

	/// <summary>
	///  Permission's should be used throughout the site to determin if something is allowed or not
	/// </summary>
	// As long as no fields of type Permission are stored in
	// the database (PermissionGroups are meant to be stored in the
	// database instead) then we can refactor and reorder the flags
	// whenever we need to.
	[Flags]
	public enum Permission
	{
		None						= 0,
		EditMyUser					= 1 << 1,
		EditUsers					= 1 << 2,
		Impersonate					= 1 << 3,
		EditUserGroups				= 1 << 4,
		EditSelectOptions			= 1 << 5,
		EditMyArtists				= 1 << 6,
		EditArtists					= 1 << 7,
		EditContests				= 1 << 8,
		MultipleArtists				= 1 << 9,
		HasWebsiteSlideshow			= 1 << 10,
		HasLongBiography			= 1 << 11,
		HasWebsiteStatus			= 1 << 12,
		CanPublishUnlimitedSongs	= 1 << 13,
		CanPublishVideos			= 1 << 14,
		CanFavoriteAndVote			= 1 << 15
	}

	public class PermissionHelper
	{
		public static Permission GetPermissionForPermissionGroup(PermissionGroup group)
		{
			Func<PermissionGroup, bool> hasPermissionGroup = (testingFor) =>
			{
				return (group & testingFor) == testingFor;
			};

			var permissionsGranted = Permission.None;

			if (hasPermissionGroup(PermissionGroup.EditMyUser))
				permissionsGranted |= Permission.EditMyUser;

			if (hasPermissionGroup(PermissionGroup.EditUsers))
				permissionsGranted |= Permission.EditUsers;

			if (hasPermissionGroup(PermissionGroup.Impersonate))
				permissionsGranted |= Permission.Impersonate;

			if (hasPermissionGroup(PermissionGroup.EditUserGroups))
				permissionsGranted |= Permission.EditUserGroups;

			if (hasPermissionGroup(PermissionGroup.EditSelectOptions))
				permissionsGranted |= Permission.EditSelectOptions;

			if (hasPermissionGroup(PermissionGroup.EditMyArtists))
				permissionsGranted |= Permission.EditMyArtists;

			if (hasPermissionGroup(PermissionGroup.EditArtists))
				permissionsGranted |= Permission.EditArtists;

			if (hasPermissionGroup(PermissionGroup.EditContests))
				permissionsGranted |= Permission.EditContests;

			if (hasPermissionGroup(PermissionGroup.MultipleArtists))
				permissionsGranted |= Permission.MultipleArtists;

			if (hasPermissionGroup(PermissionGroup.PremiumArtist))
			{
				permissionsGranted |= Permission.HasWebsiteSlideshow;
				permissionsGranted |= Permission.HasLongBiography;
				permissionsGranted |= Permission.HasWebsiteStatus;
				permissionsGranted |= Permission.CanPublishUnlimitedSongs;
				permissionsGranted |= Permission.CanPublishVideos;
			}

			if (hasPermissionGroup(PermissionGroup.CanFavoriteAndVote))
				permissionsGranted |= Permission.CanFavoriteAndVote;

			return permissionsGranted;
		}

		public static List<PermissionGroup> AllPermissionGroups()
		{
			return ((PermissionGroup[]) (typeof(PermissionGroup)).GetEnumValues()).ToList();
		}

		public static FluentPermissionGroupHelper The(PermissionGroup group)
		{
			return new FluentPermissionGroupHelper(group);
		}
	}

	public class FluentPermissionGroupHelper
	{
		public PermissionGroup PermissionGroup { get; set; }

		public FluentPermissionGroupHelper(PermissionGroup group)
		{
			PermissionGroup = group;
		}

		public PermissionGroup Add(PermissionGroup group)
		{
			return PermissionGroup | group;
		}

		public PermissionGroup RemoveAll()
		{
			return PermissionGroup.None;
		}

		public PermissionGroup Remove(PermissionGroup group)
		{
			return PermissionGroup & (~group);
		}

		public IEnumerable<PermissionGroup> ToList()
		{
			return Enum.GetValues(typeof(PermissionGroup))
						.OfType<PermissionGroup>()
						.Where(p => Includes(p));
		}

		public bool Includes(PermissionGroup group)
		{
			if (group == PermissionGroup.None) return PermissionGroup == PermissionGroup.None;
			return (PermissionGroup & group) == group;
		}
		public Permission Permissions()
		{
			return PermissionHelper.GetPermissionForPermissionGroup(PermissionGroup);
		}

		public bool Has(Permission permission)
		{
			Permission userPermission = Permissions();
			return (userPermission & permission) == permission;
		}

		public bool Have(params Permission[] permissions)
		{
			return Has(permissions);
		}

		public bool Has(params Permission[] permissions)
		{
			foreach (var permission in permissions)
			{
				if (!Has(permission)) return false;
			}
			return true;
		}

		public bool HasAny(params Permission[] permissions)
		{
			foreach (var permission in permissions)
			{
				if (Has(permission)) return true;
			}
			return false;
		}

		public IEnumerable<Permission> PermissionList()
		{
			return Enum.GetValues(typeof(Permission))
						.OfType<Permission>()
						.Where(p => Has(p));
		}

		public IEnumerable<string> PermissionNames()
		{
			return PermissionList().Select(p => Enum.GetName(typeof(Permission), p));
		}
		
		public IEnumerable<string> GroupNames()
		{
			return ToList().Select(p => Enum.GetName(typeof(PermissionGroup), p));
		}
	}

	public static class PermissionExtensions
	{
		public static PermissionGroup PermissionGroups(this IEnumerable<UserGroup> groups)
		{
			PermissionGroup result = PermissionGroup.None;
			if (groups == null || groups.Count() == 0) return result;

			groups.ToList().ForEach(p => result |= p.PermissionGroup);

			return result;
		}

		public static PermissionGroup MakeFlagset(this IEnumerable<PermissionGroup> groups)
		{
			PermissionGroup result = PermissionGroup.None;

			foreach (var group in groups)
			{
				result |= group;
			}

			return result;
		}
	}

	public interface IHasPropertyPermissions
	{
		bool GroupCanAccessProperty(PermissionGroup group, string propertyName);
	}

	public class SecureUpdater<TEntity>
		where TEntity : IHasPropertyPermissions
	{
		public ApplicationUser User { get; set; }
		public TEntity Entity { get; set; }
		public Dictionary<String, bool> PropertiesAttempted { get; set; }
		private List<SecureUpdaterPropertyChange> Changes { get; set; }

		public SecureUpdater(TEntity entity, ApplicationUser user)
		{
			Changes = new List<SecureUpdaterPropertyChange>();
			User = user;
			Entity = entity;

			PropertiesAttempted = new Dictionary<string, bool>();

			foreach (var propertyInfo in typeof(TEntity).GetProperties(BindingFlags.Public | BindingFlags.Instance))
			{
				if (propertyInfo.CanWrite)
				{
					PropertiesAttempted.Add(propertyInfo.Name, false);
				}
			}
		}

		public void Set<TProperty>(Expression<Func<TEntity, TProperty>> property, TProperty newValueP)
		{
			PropertiesAttempted[property.PropertyName()] = true;
			if (SecurityHelper.UserCanEditProperty(User, Entity, property))
			{
				PropertyInfo pi = Entity.GetType().GetProperty(property.PropertyName());
				string oldValue = (pi.GetValue(Entity) != null) ? pi.GetValue(Entity).ToString() : "";
				string newValue = (newValueP != null) ? newValueP.ToString() : "";
				if (pi.PropertyType == typeof(DateTime) || pi.PropertyType == typeof(DateTime?))
				{
					if (oldValue != null && oldValue.Right(12) == " 12:00:00 AM") oldValue = oldValue.Left(oldValue.Length - 12);
					if (newValue != null && newValue.Right(12) == " 12:00:00 AM") newValue = newValue.Left(newValue.Length - 12);
				}
				if (oldValue != newValue)
				{
					Changes.Add(new SecureUpdaterPropertyChange() { PropertyName = property.PropertyName(), OldValue = oldValue, NewValue = newValue });
				}
				pi.SetValue(Entity, newValueP);
			}
		}

		public void SkipSetting<TProperty>(Expression<Func<TEntity, TProperty>> property)
		{
			PropertiesAttempted[property.PropertyName()] = true;
		}

		public void AssertAllPropertiesAttempted()
		{
			if (PropertiesAttempted.Any(p => p.Value == false))
			{
				throw new Exception(String.Format(
					"The following properties had no attempts to set: {0}",
					String.Join(", ", PropertiesAttempted.Where(p => p.Value == false).Select(p => p.Key).ToList())));
			}
		}

		public List<SecureUpdaterPropertyChange> GetChanges()
		{
			return Changes;
		}
	}

	public class SecureUpdaterPropertyChange
	{
		public string PropertyName { get; set; }
		public string OldValue { get; set; }
		public string NewValue { get; set; }
	}

}