using URMade.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;

namespace URMade
{
    public class UserGroupIndexViewModel
    {
        public List<UserGroupViewModel> UserGroups { get; set; }
    }

    public class UserGroupViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public List<string> PermissionGroups { get; set; }

        public List<UserViewModel> Users { get; set; }

        public int UserCount { get; set; }

        public UserGroupViewModel() { }

        public UserGroupViewModel(UserGroup group, bool loadUsers = false)
        {
            Id = group.UserGroupId;
            Name = group.Name;

            PermissionGroups = group.GetPermissionGroupNames().ToList();

            Users = new List<UserViewModel>();

            if (loadUsers && group.Users != null)
            {
                UserCount = group.Users.Count();
                Users = group.Users.Select(p => new UserViewModel(p)).ToList();
            }
        }
    }

    public class EditUserGroupViewModel
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }

        [Display(Name = "Permission Groups")]
        public List<PermissionGroup> PermissionGroups { get; set; }
        [Display(Name = "Users")]
        public List<string> UserIds { get; set; }

        public List<UserSelectOptionViewModel> UserOptions { get; set; }
        public List<PermissionGroup> PermissionGroupOptions { get; set; }

        public EditUserGroupViewModel()
        {
            PermissionGroups = new List<PermissionGroup>();
        }

        public EditUserGroupViewModel(UserGroup group)
        {
            Id = group.UserGroupId;
            Name = group.Name;
            PermissionGroups = group.GetPermissionGroups().ToList();

            UserIds = new List<string>();
            if (group.Users != null)
            {
                UserIds = group.Users.Select(p => p.Id).ToList();
            }
        }
    }

    public class UserSelectOptionViewModel
    {
        public string Id { get; set; }
        public string DisplayName { get; set; }

        public UserSelectOptionViewModel() { }

        public UserSelectOptionViewModel(ApplicationUser user)
        {
            Id = user.Id;

            DisplayName = "";

            if (!string.IsNullOrWhiteSpace(user.Name))
            {
                DisplayName += user.Name + " ";
            }

            DisplayName += "<" + user.Email + ">";
        }
    }
}