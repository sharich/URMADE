using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using URMade.Models;

namespace URMade.Controllers
{
    public class ArtistController : Controller
    {
        public ArtistRepository ArtistRepo { get; set; }

        public ArtistController(ArtistRepository artistRepo)
        {
            ArtistRepo = artistRepo;
        }

		public ActionResult Index(string Name, string Type)
		{
			return View(ArtistRepo.GetTopArtistsPlaylist(Type, Name));
		}

		[RequirePermission(Permission.EditUsers)]
		public ActionResult Manage()
		{
			return View(ArtistRepo.GetArtistIndexViewModel());
		}

        [OutputCache(Duration = 10, VaryByParam = "none", VaryByCustom = "myArtist")]
        public ActionResult Website(int id = 0, string slug = "")
        {
            ArtistViewModel model = null;
            if (!string.IsNullOrWhiteSpace(slug)) model = ArtistRepo.GetArtistViewModel(slug);
            if (model==null || model.ArtistId == 0) model = ArtistRepo.GetArtistViewModel(id);
            if (model == null || model.ArtistId == 0) return Content("Artist not found");
            return View(model);
        }

        public ActionResult AddArtist()
        {
            ApplicationUser User = SecurityHelper.GetLoggedInUser();
            if (User.HasPermissionAny(Permission.EditArtists, Permission.EditMyArtists))
            {
                Artist Artist = User.Artists.FirstOrDefault();

                if (Artist == null || User.HasPermission(Permission.MultipleArtists))
					Artist = ArtistRepo.CreateNewArtistAndSave(User);

                return RedirectToAction("Website", new { Id = Artist.ArtistId });
            }
            return Content("You do not have permission to create an Artist");
        }

		[RequirePermission(Permission.EditArtists)]
		public ActionResult Delete(int id)
		{				
			ArtistRepo.DeleteArtistAndSave(id);
			return RedirectToAction("Manage", "Artist");
		}

        [HttpGet]
        public ActionResult AddSocialLink(int id)
        {
            SocialLinkViewModel model = new SocialLinkViewModel(id);

            return View(model);
        }

        [HttpPost]
        public ActionResult AddSocialLink(SocialLinkViewModel model)
        {
            if (ModelState.IsValid)
            {
                SocialLink result = ArtistRepo.AddSocialLinkAndSave(model);

                if (result.Name == "mail")
                    result.URL = "mailto:" + model.URL;
                else if (!result.URL.StartsWith("http://") && !result.URL.StartsWith("https://"))
                    result.URL = "http://" + result.URL;

                if (result != null)
                    return Json(new { SocialLinkId = result.SocialLinkId, Name = result.Name, URL = result.URL });
                else
                    return Content("Error");
            }

            return View(model);
        }

        [HttpGet]
        public ActionResult EditSocialLink(int id)
        {
            SocialLinkViewModel model = ArtistRepo.GetSocialLinkViewModel(id);

            return View(model);
        }

        [HttpPost]
        public ActionResult EditSocialLink(SocialLinkViewModel model)
        {
            if (ModelState.IsValid)
            {
                bool success = ArtistRepo.UpdateSocialLinkAndSave(model);
                if (success) return Content("Social link saved.");
                return Content("Error saving social link.");
            }

            return View(model);
        }

        [HttpPost]
        public ActionResult RemoveSocialLink(int id)
        {
            ArtistRepo.RemoveSocialLinkAndSave(id);
            return Content("");
        }

        [HttpPost]
        public ActionResult EditName(int ArtistId, string value)
        {
			string adjusted = value.Trim();

			if (adjusted.Length < 2)
				return Json(new {Status = 1, Value = ArtistRepo.GetDefaultName(ArtistId)});

			if (adjusted.Length > 32)
				adjusted = adjusted.Substring(0, 32);

			if (!adjusted.Equals(value))
			{
				ArtistRepo.SetNameAndSave(ArtistId, adjusted);
				return Json(new {Status = 0, Value = adjusted});
			}

            ArtistRepo.SetNameAndSave(ArtistId, value);
            return Json(new {Status = 0});
        }

        [HttpPost]
        public ActionResult EditShortBiography(int ArtistId, string value)
        {
            ArtistRepo.SetShortBiographyAndSave(ArtistId, value);
            return Json(new { Status = 0 });
        }

        [HttpPost]
        public ActionResult EditStatus(int ArtistId, string value)
        {
            ArtistRepo.SetStatusAndSave(ArtistId, value);
            return Json(new { Status = 0 });
        }

        [HttpPost]
        public ActionResult EditLongBiography(int ArtistId, string value)
        {
            ArtistRepo.SetLongBiographyAndSave(ArtistId, value);
            return Json(new { Status = 0 });
        }

        [HttpPost]
        public ActionResult UploadArtistImage(int ArtistId)
        {
            string url;

            if (Request.Files.Count < 1)
                return Json(new {Status = 0});

            if (ArtistRepo.SetImageAndSave(ArtistId, Request.Files.Get(0), out url))
                return Json(new {Status = 0, ImageURL = url});
            else
                return Json(new {Status = -1});
        }

        [HttpPost]
        public ActionResult UploadBannerImages(int ArtistId, string[] images)
        {
            List<string> url;

            if (ArtistRepo.SetBannerImageURLsAndSave(ArtistId, images, Request.Files, out url))
                return Json(new {Status = 0, ImageURL = url});
            else
                return Json(new {Status = -1});
        }

		[HttpPost]
		public ActionResult ChangeArtistSettings(int? artistId, string artistSlug, string artistType)
		{
			string validation = null;

			if (artistId.HasValue)
			{
				if (artistType == "Band" || artistType == "SongWriter")
					ArtistRepo.ChangeArtistTypeAndSave(artistId.Value, artistSlug, artistType, ref validation);

				var model = ArtistRepo.GetArtistViewModel(artistId.Value);
				model.SettingsValidation = validation;

				return View("Website", model);
			}
			else
				return Redirect("/");	
		}

        public JsonResult FanStatus(int id)
        {
            return Json(new { isFan = ArtistRepo.IsFanOf(id) }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult AddFan(int id)
        {
            bool isFan = ArtistRepo.AddFanAndSave(id);
            return Json(new { isFan = isFan }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult RemoveFan(int id)
        {
            bool isFan = !ArtistRepo.RemoveFanAndSave(id);
            return Json(new { isFan = isFan }, JsonRequestBehavior.AllowGet);
        }
    }
}