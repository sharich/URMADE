using Microsoft.AspNet.Identity;
using URMade.Models;
using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNet.Identity.Owin;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Mvc;

namespace URMade
{
    public class UserRepository
    {
        private ApplicationDbContext Context { get; set; }

        public UserRepository(ApplicationDbContext context)
        {
            Context = context;
        }

        private ApplicationUserManager UserManager
        {
            get
            {
                if (_userManager == null)
                {
                    try
                    {
                        _userManager = HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
                    }
                    catch
                    {
                        // When the UserRepository is used on startup before the
                        // Owin context exists.
                        _userManager = ApplicationUserManager.Create(Context);
                    }
                }

                return _userManager;
            }
        }

        private ApplicationUserManager _userManager { get; set; }

        public ApplicationUser GetUser(string id)
        {
            return Context.Users.Find(id);
        }

        public bool UserExists(string id)
        {
            return GetUser(id) != null;
        }

        public UserIndexViewModel GetUserIndexViewModel()
        {
            return new UserIndexViewModel(Context.Users);
        }

        public UserViewModel GetUserViewModel(string id)
        {
            var user = GetUser(id);

            if (user == null)
				return null;

            return new UserViewModel(user);
        }

        public UserViewModel GetUserViewModelFromSlug(string slug)
        {
            var user = Context.Users.FirstOrDefault(p => p.Slug == slug);

            if (user == null)
				return null;

            return new UserViewModel(user);
        }

        public EditUserViewModel GetEditUserViewModel(string id)
        {
            var user = GetUser(id);
            if (user == null) return null;

            return RefreshEditUserViewModel(new EditUserViewModel(user));
        }

        public EditUserViewModel GetBlankEditUserViewModel()
        {
            return RefreshEditUserViewModel(new EditUserViewModel());
        }

        public EditUserViewModel RefreshEditUserViewModel(EditUserViewModel model)
        {
            model.UserGroupOptions = Context.UserGroups.ToList();
            return model;
        }

        public ApplicationUser CreateUserAndSave(EditUserViewModel model)
        {
            return CreateUserAndSave(model.Name, model.Email, model.UserName, model.UserGroupIds);
        }

        public ApplicationUser CreateUserAndSave(string Name, string Email, string UserName, List<int> groupIds = null)
        {
            var user = new ApplicationUser();

            user.Name						= Name;
            user.Email						= Email;
			user.UserName					= Settings.UsernameSameAsEmail ? Email : UserName;
			user.FavoriteSongsArePublic		= true;
			user.FavoriteVideosArePublic	= true;
			user.FavoriteArtistsArePublic	= true;

            var result = UserManager.Create(user);

            if (groupIds != null && groupIds.Count() > 0)
            {
                user			= Context.Users.FirstOrDefault(p => p.Id == user.Id);
                user.UserGroups = Context.UserGroups.Where(p => groupIds.Contains(p.UserGroupId)).ToList();

                Context.SaveChanges();
            }

            if (!result.Succeeded)
                return null;

			return user;
        }

		public void SetupNewUserAndSave(string userId)
		{
			var user = Context.Users.FirstOrDefault(p => p.Id == userId);

			if (user == null)
				return;

			user.Name						= Settings.UsernameSameAsEmail ? user.Email.Substring(0, user.Email.IndexOf('@')) : user.UserName;
			user.FavoriteSongsArePublic		= true;
			user.FavoriteVideosArePublic	= true;
			user.FavoriteArtistsArePublic	= true;

			Context.SaveChanges();
		}

        public ApplicationUser UpdateUserAndSave(EditUserViewModel model)
        {
            ApplicationUser user = GetUser(model.Id);

            if (user == null)
				return null;

			Regex regex = new Regex("\n", RegexOptions.Multiline);

            user.Name						= model.Name;
            user.Email						= model.Email;
            user.Slug						= model.Slug;
            user.AccountType				= model.AccountType;
			user.BiographyShort				= model.BiographyShort;
			user.BiographyLong				= model.BiographyLong;
            user.FavoriteArtistsArePublic	= model.FavoriteArtistsArePublic;
            user.FavoriteSongsArePublic		= model.FavoriteSongsArePublic;
            user.FavoriteVideosArePublic	= model.FavoriteVideosArePublic;

            // Profile Photo Upload
            if (model.PhotoUpload != null)
            {
                if (!string.IsNullOrWhiteSpace(user.PhotoURL))
                {
                    AzureUploadManager.DeleteBlob(user.PhotoURL);
                    user.PhotoURL = "";
                }
                user.PhotoURL = AzureUploadManager.UploadFile(model.PhotoUpload, "ProfilePhoto");
            }
            else if (model.DeletePhoto == true && !String.IsNullOrWhiteSpace(user.PhotoURL))
            {
                AzureUploadManager.DeleteBlob(user.PhotoURL);
                user.PhotoURL = "";
            }

            if (Settings.UsernameSameAsEmail)
            {
                user.UserName = model.Email;
            }
            else
            {
                user.UserName = model.UserName;
            }

            user.UserGroups.Clear();

            if (model.UserGroupIds != null && model.UserGroupIds.Count() > 0)
            {
                Context.UserGroups.Where(p => model.UserGroupIds.Contains(p.UserGroupId))
                    .ToList().ForEach(p => user.UserGroups.Add(p));
            }
			else
			{
				var standardUserGroup = Context.UserGroups.FirstOrDefault(p => p.Name == "Standard");

				if (standardUserGroup != null)
					user.UserGroups.Add(standardUserGroup);
			}

            Context.UserGroups.Where(p => p.Name== model.AccountType)
                    .ToList().ForEach(p => user.UserGroups.Add(p));

            Context.SaveChanges();

            return user;
        }

		public ApplicationUser VerifyUserEmailAndSave(string userId)
		{
			var user = GetUser(userId);

			if (user == null)
				return null;

			// Add default "Standard" user group
			var standardUserGroup = Context.UserGroups.FirstOrDefault(p => p.Name == "Standard");

			if (standardUserGroup != null)
			{
				user.UserGroups = new List<UserGroup>();
				user.UserGroups.Add(standardUserGroup);
			}

			Context.SaveChanges();
			return user;
		}

		public bool DeleteUserAndSave(string id)
        {
			ArtistRepository artistRepo = new ArtistRepository(Context);
			SongRepository songRepo = new SongRepository(Context);
			VideoRepository videoRepo = new VideoRepository(Context);

            ApplicationUser user = GetUser(id);
            if (user == null) return false;

            // Remove Artists that have no other members
            List<Artist> ArtistsToDelete = user.Artists.Where(p=>p.Members.Count()==1).ToList();
			foreach(Artist artist in ArtistsToDelete) {
				foreach(Song s in artist.Songs.ToList())
				{
					songRepo.DeleteSong(s.SongId);
				}
				foreach(Video v in artist.Videos.ToList())
				{
					videoRepo.DeleteVideo(v);
				}
				artistRepo.DeleteArtistAndSave(artist.ArtistId);
			}
            // TODO: To support the transfer of media from one owner to another so that a user can be deleted and the media stays with the artist. Reassign ownership of media to another artist memeber.

            // Remove media not linked to an Artist
            foreach (Song s in user.Songs.Where(p=>p.Artist==null).ToList())
            {
                songRepo.DeleteSong(s.SongId);
            }
            foreach (Video v in user.Videos.Where(p => p.Artist == null).ToList())
            {
                videoRepo.DeleteVideo(v);
            }

            user.Artists.Clear();
            Context.SaveChanges();
            Context.Users.Remove(user);
            Context.SaveChanges();

            return true;
        }

        public async Task<IdentityResult> UpdatePasswordAndSave(UpdatePasswordViewModel model)
        {
            ApplicationUser user = GetUser(model.Id);
            if (user == null) return null;

            IdentityResult result = await UserManager.PasswordValidator.ValidateAsync(model.NewPassword);

            if (result.Succeeded)
            {
                UserManager.RemovePassword(user.Id);
                return UserManager.AddPassword(user.Id, model.NewPassword);
            }
            else
            {
                return result;
            }
        }

        public bool DisableLoginAndSave(string id)
        {
            ApplicationUser user = GetUser(id);
            if (user == null) return false;

            user.LoginDisabled = true;
            Context.SaveChanges();

            return true;
        }

        public bool EnableLoginAndSave(string id)
        {
            ApplicationUser user = GetUser(id);
            if (user == null) return false;

            user.LoginDisabled = false;
            Context.SaveChanges();

            return true;
        }

        public bool SendAccountResetEmail(
            UrlHelper urlHelper,
            HttpRequestBase request,
            HttpContextBase httpContext,
            string email = null,
            string accountId = null,
            string messageTitle = null,
            string messageFormatString = null,
            bool notifyOfNoAccount = false
            )
        {

            var userManager = UserManager;

            IQueryable<ApplicationUser> users;

            if (accountId != null)
            {
                users = Context.Users.Where(p => p.Id == accountId);
            }
            else if (email != null)
            {
                users = Context.Users.Where(p => p.Email == email);
            }
            else
            {
                return false;
            }

            if (users.Count() <= 0)
            {
                if (notifyOfNoAccount)
                {
                    var sb = new StringBuilder();
                    sb.AppendFormat("<p>Hello,</p>");
                    sb.AppendFormat("<p>A password reset request was submitted"
                      + " on "+Settings.SiteName+". If you did not request a password reset,"
                      + " please ignore this email.</p>");
                    sb.AppendFormat("<p>We do not have an account with your email"
                      + " address on file. If you have more than one email address,"
                      + " visit our website to submit a password reset request for"
                      + " one of your other email addresses.");
                    sb.AppendFormat("<p>Thank You,<br />" + Settings.SiteName + "</p>");

                    EmailService.SendMessage(
                      email,
                      "Password Reset Request",
                      sb.ToString());
                }
                return false;
            }
            else if (users.Count() == 1)
            {
                var user = users.First();

                string userId = user.Id;
                string code = CreateResetToken(user);

                var callbackUrl = urlHelper.Action(
                  "ResetPassword",
                  "Account",
                  new
                  {
                      t = code,
                      e = user.Email
                  },
                  protocol: request.Url.Scheme);

                messageTitle = messageTitle ?? "Password Reset Instructions";
                string message;

                if (!String.IsNullOrEmpty(messageFormatString))
                {
                    message = String.Format(messageFormatString, callbackUrl);
                }
                else
                {
                    var sb = new StringBuilder();
                    sb.AppendFormat("<p>Hello,</p>");
                    sb.AppendFormat("<p>A password reset request was submitted on"
                        + " " + Settings.SiteName + ". If you did not request a password reset,"
                        + " please ignore this email and your password will not be"
                        + " changed.</p>");
                    sb.AppendFormat("<p>To reset your password, please click the"
                        + " following link within the next 24 hours:"
                        + " <a href=\"{0}\">{0}</a></p>", callbackUrl);
                    sb.AppendFormat("<p>Thank You,<br />" + Settings.SiteName + "</p>");

                    message = sb.ToString();
                }

                try
                {
                    userManager.SendEmail(
                      userId,
                      messageTitle,
                      message);
                }
                catch
                {
                    return false;
                }

                return true;
            }
            else
            {
                List<Tuple<string, string>> resetLinks = users
                  .ToList()
                  .Select(user =>
                  {
                      string userId = user.Id;
                      string code = CreateResetToken(user);

                      var callbackUrl = urlHelper.Action(
                "ResetPassword",
                "Account",
                new
                      {
                          t = code,
                          e = user.Email
                      },
                protocol: request.Url.Scheme);

                      return new Tuple<string, string>(user.UserName, callbackUrl);
                  })
                  .ToList();

                var sb = new StringBuilder();
                sb.AppendFormat("<p>Hello,</p>");
                sb.AppendFormat("<p>A password reset request was submitted on"
                    + " " + Settings.SiteName + ". If you did not request a password reset,"
                    + " please ignore this email and your password will not be"
                    + " changed.</p>");
                sb.AppendFormat("<p>This email address has multiple accounts on "
                    + " " + Settings.SiteName + ". To reset your password, please click"
                    + " one of the following links within the next 24 hours:");

                foreach (var link in resetLinks)
                {
                    sb.AppendFormat("<p>Username: {0}<br/><a href=\"{1}\">{1}</a></p>",
                      link.Item1, link.Item2);
                }

                sb.AppendFormat("<p>Thank You,<br />" + Settings.SiteName + "</p>");

                try
                {
                    EmailService.SendMessage(
                      email,
                      "Password Reset Instructions",
                      sb.ToString());
                }
                catch
                {
                    return false;
                }

                return true;
            }
        }

		public Membership GetMembershipFromUser(string userId)
		{
			return Context.Memberships.FirstOrDefault(p => p.UserId == userId);
		}

		public MembershipViewModel GetMembershipViewModel()
		{
			var user				= SecurityHelper.GetLoggedInUser();
			Membership membership	= null;

			if (user != null)
				membership	= GetMembershipFromUser(user.Id);

			return new MembershipViewModel(membership);
		}

		public void EditMembershipAndSave(string userId, string customerId, string customerCardId, DateTime? nextPayment)
		{
			ApplicationUser user	= Context.Users.FirstOrDefault(p => p.Id == userId);
			Membership membership	= GetMembershipFromUser(userId);

			if (user == null)
				return;

			if (membership == null)
			{
				membership			= new Membership();
				membership.UserId	= userId;
				Context.Memberships.Add(membership);
			}

			if (customerCardId != membership.CustomerCardId)
				PaymentManager.DeleteCard(membership.CustomerId, membership.CustomerCardId);

			if (customerId != membership.CustomerId)
				PaymentManager.DeleteCustomer(membership.CustomerId);

			membership.CustomerId		= customerId;
			membership.CustomerCardId	= customerCardId;

			if (nextPayment.HasValue)
				membership.NextPayment = nextPayment.Value;

			UserGroup standard	= Context.UserGroups.FirstOrDefault(p => p.Name == "Standard");
			UserGroup premium	= Context.UserGroups.FirstOrDefault(p => p.Name == "Premium");

			if (membership.NextPayment > DateTime.Today)
			{
				if (!user.UserGroups.Contains(premium))
					user.UserGroups.Add(premium);

				user.UserGroups.Remove(standard);
			}
			else
			{
				if (!user.UserGroups.Contains(standard))
					user.UserGroups.Add(standard);

				user.UserGroups.Remove(premium);
			}

			Context.SaveChanges();
		}

		public void ToggleMemberAutoRenewalAndSave(string id)
		{
			var membership = GetMembershipFromUser(id);

			if (membership == null)
				return;

			membership.PendingCancel = !membership.PendingCancel;
			Context.SaveChanges();
		}

		public string UpdateMemberships()
		{
			UserGroup standard	= Context.UserGroups.FirstOrDefault(p => p.Name == "Standard");
			UserGroup premium	= Context.UserGroups.FirstOrDefault(p => p.Name == "Premium");

			var memberships		= Context.Memberships.ToList();
			string chargeResult = null;
			string output		= "";

			foreach (var membership in memberships)
			{
				ApplicationUser user	= Context.Users.FirstOrDefault(p => p.Id == membership.UserId);
				DateTime next			= membership.NextPayment;

				if (next < DateTime.Today && !string.IsNullOrWhiteSpace(membership.CustomerId) && !string.IsNullOrWhiteSpace(membership.CustomerCardId))
				{
					if (membership.PendingCancel)
					{
						PaymentManager.DeleteCard(membership.CustomerId, membership.CustomerCardId);
						PaymentManager.DeleteCustomer(membership.CustomerId);

						membership.CustomerId		= null;
						membership.CustomerCardId	= null;
						membership.PendingCancel	= false;

						output += "User (" + user.Id + ")'s membership renewal was canceled.<br/>";
					}
					else if (PaymentManager.ChargeCard(membership.CustomerId, membership.CustomerCardId, 14.95m, "USD", ref chargeResult))
					{
						next					= DateTime.Today.AddDays(30);
						membership.NextPayment	= next;

						output += "User (" + user.Id + ")'s membership renewal succeeded.<br/>";
					}
					else
						output += "User (" + user.Id + ")'s membership renewal failed with error \"" + chargeResult + "\"<br/>";
				}

				if (next < DateTime.Today)
				{
					if (!user.UserGroups.Contains(standard))
						user.UserGroups.Add(standard);

					user.UserGroups.Remove(premium);
				}
				else
				{
					if (!user.UserGroups.Contains(premium))
						user.UserGroups.Add(premium);

					user.UserGroups.Remove(standard);
				}
			}

			Context.SaveChanges();
			return output;
		}

		public string CreateResetToken(ApplicationUser user)
        {
            user.PasswordResetToken = Guid.NewGuid().ToString().Replace("-", "");
            user.PasswordResetTokenExpires = DateTime.UtcNow.AddHours(24);
            Context.SaveChanges();

            return user.PasswordResetToken;
        }

	}
}