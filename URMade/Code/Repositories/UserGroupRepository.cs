
using URMade.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace URMade
{
    public class UserGroupRepository
    {
        private ApplicationDbContext Context { get; set; }

        public UserGroupRepository(ApplicationDbContext context)
        {
            Context = context;
        }

        public bool Exists(int id)
        {
            return Get(id) != null;
        }

        public UserGroup Get(int id)
        {
            return Context.UserGroups.Find(id);
        }

        public UserGroupIndexViewModel GetUserGroupIndexViewModel()
        {
            var model = new UserGroupIndexViewModel();

            model.UserGroups = Context.UserGroups.ToList()
                .Select(p => new UserGroupViewModel(p, true)).ToList();

            return model;
        }

        public UserGroupViewModel GetUserGroupViewModel(int id)
        {
            UserGroup group = Get(id);
            if (group == null) return null;

            return new UserGroupViewModel(group, true);
        }

        public EditUserGroupViewModel RefreshEditUserGroupViewModel(
            EditUserGroupViewModel model)
        {
            model.UserOptions = Context.Users.ToList()
                .Select(p => new UserSelectOptionViewModel(p)).ToList();

            model.PermissionGroupOptions = PermissionHelper.AllPermissionGroups();
            model.PermissionGroupOptions.Remove(PermissionGroup.None);

            return model;
        }

        public EditUserGroupViewModel GetEditUserGroupViewModel(int id)
        {
            UserGroup group = Get(id);
            if (group == null) return null;

            var model = new EditUserGroupViewModel(group);
            RefreshEditUserGroupViewModel(model);

            return model;
        }

        public EditUserGroupViewModel GetBlankEditUserGroupViewModel()
        {
            return RefreshEditUserGroupViewModel(new EditUserGroupViewModel());
        }

        public UserGroup CreateUserGroupAndSave(EditUserGroupViewModel model)
        {
            var group = new UserGroup();

            group.Name = model.Name;
            group.PermissionGroup = model.PermissionGroups.MakeFlagset();

            Context.UserGroups.Add(group);
            Context.SaveChanges();

            if (model.UserIds != null && model.UserIds.Count() > 0)
            {
                group.Users = new List<ApplicationUser>();
                var users = Context.Users.Where(p => model.UserIds.Contains(p.Id)).ToList();
                foreach (var user in users)
                {
                    group.Users.Add(user);
                }

                Context.SaveChanges();
            }

            return group;
        }

        public UserGroup UpdateUserGroupAndSave(EditUserGroupViewModel model)
        {
            UserGroup group = Get(model.Id);
            if (group == null) return null;

            group.Name = model.Name;
            group.PermissionGroup = model.PermissionGroups.MakeFlagset();

            group.Users.Clear();

            if (model.UserIds != null && model.UserIds.Count() > 0)
            {
                var users = Context.Users.Where(p => model.UserIds.Contains(p.Id)).ToList();
                foreach (var user in users)
                {
                    group.Users.Add(user);
                }
            }

            Context.SaveChanges();

            return group;
        }

		public void ResetPermissionsAndSave()
		{
			UserGroup group;

			// Standard
			group = Context.UserGroups.FirstOrDefault(p => p.Name == "Standard");

			if (group == null)
			{
				group = new UserGroup() {Name = "Standard"};
				Context.UserGroups.Add(group);
			}

			group.PermissionGroup = PermissionGroup.EditMyUser |
									PermissionGroup.EditMyArtists;

			// Premium
			group = Context.UserGroups.FirstOrDefault(p => p.Name == "Premium");

			if (group == null)
			{
				group = new UserGroup() {Name = "Premium"};
				Context.UserGroups.Add(group);
			}

			group.PermissionGroup = PermissionGroup.EditMyUser		|
									PermissionGroup.EditMyArtists	|
									PermissionGroup.MultipleArtists |
									PermissionGroup.PremiumArtist;

			// Administrator
			group = Context.UserGroups.FirstOrDefault(p => p.Name == "Admin");

			if (group == null)
			{
				group = new UserGroup() {Name = "Admin"};
				Context.UserGroups.Add(group);
			}

			group.PermissionGroup = PermissionGroup.EditMyUser		|
									PermissionGroup.EditUsers		|
									PermissionGroup.EditUserGroups	|
									PermissionGroup.EditMyArtists	|
									PermissionGroup.EditArtists		|
									PermissionGroup.MultipleArtists |
									PermissionGroup.PremiumArtist;

			// Developer
			group = Context.UserGroups.FirstOrDefault(p => p.Name == "Developer");

			if (group == null)
			{
				group = new UserGroup() {Name = "Developer"};
				Context.UserGroups.Add(group);
			}

			group.PermissionGroup = PermissionGroup.EditMyUser		|
									PermissionGroup.EditUsers		|
									PermissionGroup.EditUserGroups	|
									PermissionGroup.EditMyArtists	|
									PermissionGroup.EditArtists		|
									PermissionGroup.MultipleArtists |
									PermissionGroup.Impersonate		|
									PermissionGroup.PremiumArtist;
		}

		public bool DeleteUserGroupAndSave(int id)
        {
            UserGroup group = Get(id);
            if (group == null) return false;

            Context.UserGroups.Remove(group);
            Context.SaveChanges();

            return true;
        }
    }
}