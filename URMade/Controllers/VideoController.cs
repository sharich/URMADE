using System.Net;
using System.Web.Mvc;
using URMade.Models;

namespace URMade.Controllers
{
    public class VideoController : Controller
    {
		private VideoRepository VideoRepo {get; set;}

		public VideoController(VideoRepository videoRepo)
		{
			VideoRepo = videoRepo;
		}

        [OutputCache(Duration = 10, VaryByParam = "*", VaryByCustom = "user")]
        public ActionResult Index(PlaylistQuery options)
        {
			return View(VideoRepo.GetVideoPlaylist(options));
        }

		public ActionResult MyVideos()
		{
			return View("Index", VideoRepo.GetUserVideoPlaylist(new PlaylistQuery()));
		}

		public ActionResult Create()
		{
			var user = SecurityHelper.GetLoggedInUser();

			if (user == null)
				return RedirectToAction("Index", "Home");

			return View(new EditVideoViewModel());
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Create(EditVideoViewModel model)
		{
			if (model.Video == null)
				ModelState.AddModelError("Video", "A video file must be selected.");
            else if (!AzureUploadManager.ValidUploadExtension("Video", model.Video))
                ModelState.AddModelError("Video", "The selected video format is not supported.");

            if (!AzureUploadManager.ValidUploadExtension("AlbumArt", model.AlbumArt))
                ModelState.AddModelError("AlbumArt", "The selected album art format is not supported.");

            if (!ModelState.IsValid)
				return View(model);

			var user = SecurityHelper.GetLoggedInUser();

			if (user == null)
				return RedirectToAction("Index", "Home");

			VideoRepo.EditVideoAndSave(model);

            return RedirectToAction("MyVideos");
		}

        public ActionResult Edit(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            Video video = VideoRepo.GetVideo(id.Value);

            if (video == null)
                return HttpNotFound();

            return View(new EditVideoViewModel(video));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(EditVideoViewModel model)
        {
            if (!ModelState.IsValid)
				return View(model);

			VideoRepo.EditVideoAndSave(model);
            return RedirectToAction("MyVideos");
        }

		public ActionResult Delete(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            Video video = VideoRepo.GetVideo(id.Value);

            if (video == null)
                return HttpNotFound();

            return View(video);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
			VideoRepo.DeleteVideoAndSave(id);
            return RedirectToAction("MyVideos");
        }

		public ActionResult Load(PlaylistQuery query)
		{
			return Json(VideoRepo.GetVideoPlaylist(query), JsonRequestBehavior.AllowGet);
		}

		public ActionResult Favorite(int id)
		{
			bool active;
			int count = VideoRepo.ToggleFavoriteAndSave(id, out active);

			return Json(new {Active = active, FanCount = count});
		}

		[AllowAnonymous]
        public ActionResult Queue(string key)
        {
			if (key == "zIfJM6Wn4xWNUSqCnkEq")
			{
				VideoRepo.UpdateQueueAndSave();
				return Content("");
			}

            return Redirect("/");
        }
    }
}