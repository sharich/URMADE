using URMade.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace URMade
{
    public class UserIndexViewModel
    {
        public List<UserViewModel> AllUsers { get; set; }

        public UserIndexViewModel() { }

        public UserIndexViewModel(IEnumerable<ApplicationUser> users)
        {
            AllUsers = new List<UserViewModel>();

            if (users != null && users.Count() > 0)
            {
                AllUsers = users.ToList().Select(p => new UserViewModel(p)).ToList();
            }

        }
    }

    public class EditUserViewModel
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Name { get; set; }

        [Required]
        public string Email { get; set; }

        [Display(Name = "Account Type")]
        public string AccountType { get; set; }

        [Display(Name = "Profile URL")]
        public string Slug { get; set; }

        [Display(Name = "Profile Photo")]
        public string PhotoURL { get; set; }
        public System.Web.HttpPostedFileBase PhotoUpload { get; set; }
        public bool DeletePhoto { get; set; }

        [Display(Name = "Biography")]
        public string BiographyShort { get; set; }
        [Display(Name = "Biography")]
        public string BiographyLong { get; set; }

        [Display(Name = "My favorite songs are visible to the public")]
        public bool FavoriteSongsArePublic { get; set; }
        [Display(Name = "My favorite videos are visible to the public")]
        public bool FavoriteVideosArePublic { get; set; }
        [Display(Name = "My favorite artists are visible to the public")]
        public bool FavoriteArtistsArePublic { get; set; }

		public bool CanManageUsers		{get; set;}
		public bool AddArtist			{get; set;}
		public bool ShowLongBiography	{get; set;}

        public List<int> UserGroupIds { get; set; }
        public List<UserGroup> UserGroupOptions { get; set; }

        public EditUserViewModel() {}

        public EditUserViewModel(ApplicationUser user)
        {
            if (user != null)
            {
				var current = SecurityHelper.GetLoggedInUser();

                Id							= user.Id;
                UserName					= user.UserName;
                Email						= user.Email;
                Name						= user.Name;
                Slug						= user.Slug;
                PhotoURL					= user.PhotoURL;
                AccountType					= user.AccountType;
				BiographyShort				= user.BiographyShort;
				BiographyLong				= user.BiographyLong;
                FavoriteArtistsArePublic	= user.FavoriteArtistsArePublic;
                FavoriteSongsArePublic		= user.FavoriteSongsArePublic;
                FavoriteVideosArePublic		= user.FavoriteVideosArePublic;

				CanManageUsers		= current != null ? current.HasPermission(Permission.EditArtists) : false;
				ShowLongBiography	= user.HasPermission(Permission.HasLongBiography);

                if (user.UserGroups != null)
                    UserGroupIds = user.UserGroups.Select(p => p.UserGroupId).ToList();
                else
                    UserGroupIds = new List<int>();
            }
        }

		public bool validate(out string field, out string message)
		{
			if (string.IsNullOrWhiteSpace(Name))
			{
				field	= "Name";
				message = "Name cannot be blank or whitespace.";

				return false;
			}

			Name = Name.Trim();

			field	= null;
			message = null;

			return true;
		}
    }

    public class UserViewModel
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }

        [Display(Name = "Account Type")]
        public string AccountType { get; set; }

        [Display(Name = "Profile URL")]
        public string ProfileURL { get; set; }

        [Display(Name = "Profile Photo")]
        public string PhotoURL { get; set; }

		[Display(Name = "Biography")]
		public string Biography {get; set;}

        public List<ArtistResultViewModel> Artists { get; set; }

        [Display(Name = "Favorite songs are visible to the public")]
        public bool FavoriteSongsArePublic { get; set; }
        public List<Song> FavoriteSongs { get; set; }

        [Display(Name = "Favorite videos are visible to the public")]
        public bool FavoriteVideosArePublic { get; set; }
        public List<Video> FavoriteVideos { get; set; }

        [Display(Name = "Favorite artists are visible to the public")]
        public bool FavoriteArtistsArePublic { get; set; }
        public List<ArtistResultViewModel> FavoriteArtists { get; set; }

        public bool IsLoggedInUser { get; set; }
        public bool IsAdmin { get; set; }

		// TODO: Make model permissions into bitwise flag with properties to get/set value for performance.

		// Permissions
		public bool CanManageUsers			{get; set;}
		public bool CanImpersonate			{get; set;}
		public bool CanEditUser				{get; set;}
		public bool CanEditArtists			{get; set;}
		public bool CanHaveMultipleArtists	{get; set;}

        public bool Disabled { get; set; }
        public string Name { get; set; }
        public List<UserGroup> UserGroups { get; set; }
        public UpdatePasswordViewModel UpdatePassword { get; set; }

        public UserViewModel()
        {
            UpdatePassword = new UpdatePasswordViewModel();
        }

        public UserViewModel(ApplicationUser user)
        {
            UpdatePassword = new UpdatePasswordViewModel();

            if (user != null)
            {
				var current = SecurityHelper.GetLoggedInUser();

                UpdatePassword.Id	= user.Id;
                IsLoggedInUser		= (current != null && current.Id == user.Id);

				// Permissions
				if (IsLoggedInUser)
				{
					CanManageUsers		= false;
					CanImpersonate		= false;
					CanEditUser			= user.HasPermissionAny(Permission.EditMyUser, Permission.EditUsers);
					CanEditArtists		= user.HasPermissionAny(Permission.EditMyArtists, Permission.EditArtists);
				}
				else if (current != null)
				{
					CanManageUsers	= current.HasPermission(Permission.EditUsers);
					CanImpersonate	= current.HasPermission(Permission.Impersonate);
					CanEditUser		= CanManageUsers;
					CanEditArtists	= current.HasPermission(Permission.EditArtists);
				}
				else
				{
					CanManageUsers	= false;
					CanImpersonate	= false;
					CanEditUser		= false;
					CanEditArtists	= false;
				}

				CanHaveMultipleArtists	= user.HasPermission(Permission.MultipleArtists);

				FavoriteSongsArePublic		= user.FavoriteSongsArePublic;
				FavoriteVideosArePublic		= user.FavoriteVideosArePublic;
				FavoriteArtistsArePublic	= user.FavoriteArtistsArePublic;

				//
                Id				= user.Id;
                UserGroups		= user.UserGroups == null ? new List<UserGroup>() : user.UserGroups.ToList();
                UserName		= user.UserName;
                Email			= user.Email;
                Name			= user.Name;
                ProfileURL		= user.Slug;
                PhotoURL		= user.PhotoURL;
                AccountType		= user.AccountType;
				Biography		= user.HasPermission(Permission.HasLongBiography) ? user.BiographyLong : user.BiographyShort;
				Biography		= !string.IsNullOrWhiteSpace(Biography) ? Biography.Replace("\n", "<br />") : null;
                Artists			= user.Artists.Select(p => new ArtistResultViewModel(p)).ToList();
				Disabled		= user.LoginDisabled;

				// Retrieve song, video, and artist favorite lists.
                if (FavoriteSongsArePublic || CanEditUser)
					FavoriteSongs = user.FavoriteSongs.ToList();
				else
					FavoriteSongs = null;

                if (FavoriteVideosArePublic || CanEditUser)
					FavoriteVideos = user.FavoriteVideos.ToList();
				else
					FavoriteVideos = null;

                if (FavoriteArtistsArePublic || CanEditUser)
					FavoriteArtists = user.FavoriteArtists.Select(p=>new ArtistResultViewModel(p)).ToList();
				else
					FavoriteArtists = null;
            }
        }
    }

    public class UpdatePasswordViewModel
    {
        [Required]
        public string Id { get; set; }
        [Required, Display(Name = "New Password")]
        public string NewPassword { get; set; }
        [Required, Display(Name = "Confirm New Password")]
        [System.ComponentModel.DataAnnotations.Compare("NewPassword")]
        public string ConfirmNewPassword { get; set; }
    }
}