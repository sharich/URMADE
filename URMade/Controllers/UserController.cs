using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using URMade.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace URMade.Controllers
{
	using System.Web.Helpers;

    public class UserController : Controller
    {
        public UserRepository UserRepo { get; set; }
        public ApplicationUserManager UserManager { get; set; }

        public UserController(UserRepository userRepo, ApplicationUserManager userManager)
        {
            UserRepo = userRepo;
            UserManager = userManager;
        }

		[Authorize]
		[RequirePermission(Permission.EditUsers)]
        public ActionResult Index()
        {
            UserIndexViewModel model = UserRepo.GetUserIndexViewModel();

            return View(model);
        }
		
        public ActionResult Details(string id = "", string slug = "")
        {
			if (string.IsNullOrWhiteSpace(id) && string.IsNullOrWhiteSpace(slug))
				id = SecurityHelper.GetLoggedInUser().Id;

            UserViewModel model = null;

            if (!string.IsNullOrWhiteSpace(id))
                model = UserRepo.GetUserViewModel(id);
            else if (!string.IsNullOrWhiteSpace(slug))
                model = UserRepo.GetUserViewModelFromSlug(slug);

            if (model == null) return HttpNotFound();

            return View(model);
        }

        [HttpGet]
		[OverrideDefaultPermission]
        public ActionResult Edit(string id, bool AddArtist = false)
        {
			if (SecurityHelper.IsLoggedInWithPermission(id, Permission.EditMyUser, Permission.EditUsers))
			{
				EditUserViewModel model = UserRepo.GetEditUserViewModel(id);

				if (model == null)
					return HttpNotFound();

				model.AddArtist = AddArtist;

				return View(model);
			}

            return Redirect("/");
        }

        [HttpPost]
		[OverrideDefaultPermission]
        public ActionResult Edit(EditUserViewModel model)
        {
			if (SecurityHelper.IsLoggedInWithPermission(model.Id, Permission.EditMyUser, Permission.EditUsers))
			{
				string field, message;

				if (!model.validate(out field, out message))
					ModelState.AddModelError(field, message);
				else if (ModelState.IsValid)
				{
					ApplicationUser user = UserRepo.UpdateUserAndSave(model);

					if (user != null)
					{
						if (model.AddArtist)
							return RedirectToAction("AddArtist", "Artist");
						else
							return RedirectToAction("Details", new { id = user.Id });
					}
				}

				UserRepo.RefreshEditUserViewModel(model);
				return View(model);
			}

			return Redirect("/");
        }

        [HttpGet]
		[Authorize]
		[RequirePermission(Permission.EditUsers)]
        public ActionResult Add()
        {
            EditUserViewModel model = UserRepo.GetBlankEditUserViewModel();
            return View(model);
        }

        [HttpPost]
		[Authorize]
		[RequirePermission(Permission.EditUsers)]
        public ActionResult Add(EditUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                ApplicationUser user = UserRepo.CreateUserAndSave(model);
                if (user != null)
                {
                    return RedirectToAction("Details", new { id = user.Id });
                }
            }

            UserRepo.RefreshEditUserViewModel(model);
            return View();
        }

		[OverrideDefaultPermission]
		public ActionResult ChangePassword(string id)
		{
			if (SecurityHelper.IsLoggedInWithPermission(id, Permission.EditMyUser, Permission.EditUsers))
				return View(new UserViewModel(UserRepo.GetUser(id)));

			return Redirect("/");
		}

        [HttpPost]
        [OverrideDefaultPermission]
        public async Task<ActionResult> UpdatePassword(string id, [Bind(Prefix = "UpdatePassword")]UpdatePasswordViewModel model)
        {
            if (SecurityHelper.LoggedInUserHas(Permission.EditUsers) == false)
            {
                id = SecurityHelper.GetLoggedInUser().Id;
                model.Id = id;
            }

            if (ModelState.IsValid && UserRepo.UserExists(model.Id))
            {
                IdentityResult result = await UserRepo.UpdatePasswordAndSave(model);
                if (result.Succeeded)
                {
                    this.FlashSuccessNotification("User password has been updated.");
                }
                else
                {
                    this.FlashErrorNotification("Unable to update password. " + result.Errors.First());
                }

                return RedirectToAction("Details", new { id = id });
            }

            this.FlashErrorNotification(ModelState.FirstErrorMessage());
            return RedirectToAction("Details", new { id = id });
        }

        [HttpPost]
		[Authorize]
		[RequirePermission(Permission.EditUsers)]
        public ActionResult DisableLogin(string id)
        {
            if (UserRepo.DisableLoginAndSave(id))
            {
                this.FlashSuccessNotification("User login has been disabled.");
            }
            else
            {
                this.FlashErrorNotification("There was an error disabling the user's login.");
            }

            return RedirectToAction("Details", new { id = id });
        }

        [HttpPost]
		[Authorize]
		[RequirePermission(Permission.EditUsers)]
        public ActionResult EnableLogin(string id)
        {
            if (UserRepo.EnableLoginAndSave(id))
            {
                this.FlashSuccessNotification("User login has been enabled.");
            }
            else
            {
                this.FlashErrorNotification("There was an error enabling the user's login.");
            }

            return RedirectToAction("Details", new { id = id });
        }

        [HttpGet]
		[Authorize]
		[RequirePermission(Permission.EditUsers)]
        public ActionResult Delete(string id)
        {
            UserViewModel model = UserRepo.GetUserViewModel(id);
            if (model == null) return HttpNotFound();

            return View(model);
        }

		[Authorize]
		[RequirePermission(Permission.EditUsers)]
        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(string id)
        {
            if (UserRepo.DeleteUserAndSave(id))
            {
                this.FlashSuccessNotification("User has been deleted.");
            }
            else
            {
                this.FlashErrorNotification("There was an error deleting the requested user.");
            }

            return RedirectToAction("Index");
        }

		[OverrideDefaultPermission]
		public ActionResult FirstLogin()
		{
			return View();
		}

		private string GetAntiForgeryToken()
		{
			string token	= AntiForgery.GetHtml().ToString();
			int start		= token.LastIndexOf("value=\"") + 7;
			int end			= token.IndexOf("\"", start);

			return token.Substring(start, end - start);
		}

		[Authorize]
		public ActionResult Header()
		{
			var currentUser	= SecurityHelper.GetLoggedInUser();
			var result		= new Dictionary<string, object>();

			if (currentUser != null)
			{
				result["Username"]	= Settings.UsernameSameAsEmail ? currentUser.Email : currentUser.UserName;
				result["Token"]		= GetAntiForgeryToken();

				var Account		= new List<string>();
				var Websites	= new List<KeyValuePair<string, int>>();

				Account.Add("Profile");

				if (currentUser.Artists != null && currentUser.Artists.Count > 0)
				{
					Account.Add("My Songs");
					Account.Add("My Videos");

					if (currentUser.HasPermission(Permission.MultipleArtists))
					{
						foreach (Artist artist in currentUser.Artists)
							Websites.Add(new KeyValuePair<string, int>(string.IsNullOrWhiteSpace(artist.Name) ? "Artist" : artist.Name, artist.ArtistId));
					}
					else
					{
						var artist = currentUser.Artists.FirstOrDefault();

						if (artist != null)
							Websites.Add(new KeyValuePair<string, int>(string.IsNullOrWhiteSpace(artist.Name) ? "Artist" : artist.Name, artist.ArtistId));
					}
				}

				if (currentUser.HasPermissionAny(Permission.EditUsers,
												 Permission.EditArtists,
												 Permission.EditUserGroups,
												 Permission.EditSelectOptions,
												 Permission.EditContests))
				{
					var Manage = new List<string>();

					if (currentUser.HasPermission(Permission.EditUsers))
						Manage.Add("Users");

					if (currentUser.HasPermission(Permission.EditArtists))
						Manage.Add("Artists");

					if (currentUser.HasPermission(Permission.EditUserGroups))
						Manage.Add("User Groups");

					if (currentUser.HasPermission(Permission.EditSelectOptions))
						Manage.Add("Select Options");

					if (currentUser.HasPermission(Permission.EditContests))
						Manage.Add("Contests");

					result["Manage"] = Manage;
				}

				result["Account"]	= Account;
				result["Websites"]	= Websites;
			}
			else
				result["ErrorMessage"] = "No user logged in this session.";

			return Json(result, JsonRequestBehavior.AllowGet);
		}

		[HttpGet]
		[OverrideDefaultPermission]
		public ActionResult Membership()
		{
			return View(UserRepo.GetMembershipViewModel());
		}

		[HttpPost]
		[OverrideDefaultPermission]
		public ActionResult Membership(MembershipViewModel model)
		{
			var user = SecurityHelper.GetLoggedInUser();

			if (user == null)
				return Redirect("/");

			bool success		= false;
			string customerId	= null;
			string cardId		= null;
			string msg			= null;

			if (model.UpdateCardInformation)
			{
				customerId = PaymentManager.CreateCustomer(model.FirstName, model.LastName, model.Email, model.PhoneNumber);

				if (customerId != null)
					cardId = PaymentManager.CreateCustomerCard(model.CardNonce, null, model.NameOnCard, customerId);

				if (cardId != null)
					success = PaymentManager.Authenticate(customerId, cardId, ref msg);

				if (!success)
				{
					if (msg != null)
						ModelState.AddModelError("Authentication", msg);

					ModelState.AddModelError("Authentication", "Your card failed authentication. Check your card information and try again!");
				}
				else
					UserRepo.EditMembershipAndSave(user.Id, customerId, cardId, null);

				return View(UserRepo.GetMembershipViewModel());
			}
			else if (model.ToggleAutomaticRenewal)
			{
				UserRepo.ToggleMemberAutoRenewalAndSave(user.Id);
				return View(UserRepo.GetMembershipViewModel());
			}

			if (string.IsNullOrWhiteSpace(model.FirstName))
				ModelState.AddModelError("FirstName", "First name is required");

			if (string.IsNullOrWhiteSpace(model.LastName))
				ModelState.AddModelError("LastName", "First name is required");

			if (string.IsNullOrWhiteSpace(model.Email))
				ModelState.AddModelError("Email", "Email address is required");

			if (string.IsNullOrWhiteSpace(model.NameOnCard))
				ModelState.AddModelError("NameOnCard", "Name on card is required");

			if (!ModelState.IsValid)
				return View(model);

			customerId = PaymentManager.CreateCustomer(model.FirstName, model.LastName, model.Email, model.PhoneNumber);

			if (customerId != null)
				cardId = PaymentManager.CreateCustomerCard(model.CardNonce, null, model.NameOnCard, customerId);
				
			if (cardId != null)
				success = PaymentManager.ChargeCard(customerId, cardId, 14.95m, "USD", ref msg);
			
			if (!success)
			{
				if (msg != null)
					ModelState.AddModelError("Payment", msg);

				ModelState.AddModelError("Payment", "We were unable to complete the transaction. Check your card information and try again!");

				if (customerId != null)
					PaymentManager.DeleteCustomer(customerId);

				return View(model);
			}

			UserRepo.EditMembershipAndSave(user.Id, customerId, cardId, DateTime.Today.AddDays(30));
			return View(UserRepo.GetMembershipViewModel());
		}

		[OverrideDefaultPermission]
		[AllowAnonymous]
		public ActionResult UpdateMemberships(string key)
		{
			if (key == "zIfJM6Wn4xWNUSqCnkEq")
				return Content(UserRepo.UpdateMemberships());

			return Redirect("/");
		}

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

		[Authorize]
        public async Task<ActionResult> Impersonate(string id)
        {
            if (SecurityHelper.LoggedInUserHas(Permission.Impersonate))
            {
                ApplicationUser user = UserRepo.GetUser(id);

                if (user != null)
                {
                    AuthenticationManager.SignOut(DefaultAuthenticationTypes.ExternalCookie);
                    var identity = await UserManager.CreateIdentityAsync(user, DefaultAuthenticationTypes.ApplicationCookie);
                    AuthenticationManager.SignIn(new AuthenticationProperties()
                    {
                        IsPersistent = false
                    }, identity);
                }
            }

            return RedirectToAction("Index", "Home", null);
        }

        [HttpPost]
		[Authorize]
		[RequirePermission(Permission.EditUsers)]
        public ActionResult SendPasswordResetEmail(string id)
        {
            var user = UserRepo.GetUser(id);
            if (User == null) return HttpNotFound();
            if (String.IsNullOrWhiteSpace(user.Email))
            {
                this.FlashErrorNotification("Can't send Password Reset Email because user does not have an email address.");
                return RedirectToAction("Details", new { id = id });
            }

            if (UserRepo.SendAccountResetEmail(
                Url, Request, HttpContext,
                accountId: id))
            {
                this.FlashSuccessNotification("Password Reset Email has been sent.");
                return RedirectToAction("Details", new { id = id });
            }
            else
            {
                this.FlashErrorNotification("There was a problem sending the"
                    + " password reset email. Please check that the login email is"
                    + " a valid email address and try again.");
                return RedirectToAction("Details", new { id = id });
            }
        }

        [HttpPost]
		[Authorize]
		[RequirePermission(Permission.EditUsers)]
        public ActionResult SendNewAccountEmail(string id)
        {
            var user = UserRepo.GetUser(id);
            if (User == null) return HttpNotFound();
            if (String.IsNullOrWhiteSpace(user.Email))
            {
                this.FlashErrorNotification("Can't send New Account Email because user does not have an email address.");
                return RedirectToAction("Details", new { id = id });
            }

            var format = @"
            <p>Hello,</p>
            <p>An account has been created for you at " + Settings.SiteName + @".</p>
            < p>Your username is: " + user.UserName + @"</p>
            <p>To set your password and sign-in, please click the following
            link in the next 24 hours: <a href=""{0}"">{0}</a></p>
            <p>Thank You,<br/>" + Settings.SiteName + @"</p>";

            if (UserRepo.SendAccountResetEmail(
                Url, Request, HttpContext,
                accountId: id,
                messageTitle: "New Account for " + Settings.SiteName + @"",
                messageFormatString: format))
            {
                this.FlashSuccessNotification("New Account Email has been sent.");
                return RedirectToAction("Details", new { id = id });
            }
            else
            {
                this.FlashErrorNotification("There was a problem sending the"
                    + " new account email. Please check that the login email is"
                    + " a valid email address and try again.");
                return RedirectToAction("Details", new { id = id });
            }
        }
    }
}