using System.Web;
using System.Net;
using System.Web.Mvc;
using System.Collections.Generic;
using URMade.Models;

namespace URMade.Controllers
{
    public class SongController : AsyncController
    {
		private SongRepository SongRepo {get; set;}

		public SongController(SongRepository songRepo)
		{
			SongRepo = songRepo;
		}

        [OutputCache(Duration = 10, VaryByParam = "*", VaryByCustom = "user")]
        public ActionResult Index(PlaylistQuery options)
        {
			return View(SongRepo.GetSongPlaylist(options));
        }

		public ActionResult MySongs()
		{
			var model = SongRepo.GetUserSongPlaylist(new PlaylistQuery());
			return View("Index", model);
		}

        public ActionResult Details(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

			Song song = SongRepo.GetSong(id.Value);

            if (song == null)
                return HttpNotFound();

            return View(song);
        }

        // GET: Song/Create
        public ActionResult Create()
        {
			var user = SecurityHelper.GetLoggedInUser();

			if (user == null)
				return RedirectToAction("Index", "Home");

            return View(new EditSongViewModel());
        }

        // POST: Song/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(EditSongViewModel model)
        {
			if (model.Audio == null)
				ModelState.AddModelError("Audio", "An audio file must be selected.");
            else if (!AzureUploadManager.ValidUploadExtension("Audio", model.Audio))
                ModelState.AddModelError("Audio", "The selected audio format is not supported.");

            if (!AzureUploadManager.ValidUploadExtension("AlbumArt", model.AlbumArt))
                ModelState.AddModelError("AlbumArt", "The selected album art format is not supported.");

            if (!ModelState.IsValid)
				return View(model);

			var user = SecurityHelper.GetLoggedInUser();

			if (user == null)
				return RedirectToAction("Index", "Home");

			SongRepo.EditSongAndSave(model);
            return RedirectToAction("MySongs");
        }

        // GET: Song/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            Song song = SongRepo.GetSong(id.Value);

            if (song == null)
                return HttpNotFound();

            return View(new EditSongViewModel(song));
        }

        // POST: Song/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(EditSongViewModel model)
        {
            if (!ModelState.IsValid)
				return View(model);

			SongRepo.EditSongAndSave(model);
            return RedirectToAction("MySongs");
        }

        public ActionResult Delete(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            Song song = SongRepo.GetSong(id.Value);

            if (song == null)
                return HttpNotFound();

            return View(song);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
			SongRepo.DeleteSongAndSave(id);
			return RedirectToAction("MySongs");
        }

		public ActionResult Load(PlaylistQuery query)
		{
			return Json(SongRepo.GetSongPlaylist(query), JsonRequestBehavior.AllowGet);
		}

		public ActionResult Favorite(int id)
		{
			bool active;
			int count = SongRepo.ToggleFavoriteAndSave(id, out active);

			return Json(new {Active = active, FanCount = count});
		}

		[AllowAnonymous]
        public ActionResult Queue(string key)
        {
			if (key == "zIfJM6Wn4xWNUSqCnkEq")
			{
				SongRepo.UpdateQueueAndSave();
				return Content("");
			}

            return Redirect("/");
        }
    }
}
