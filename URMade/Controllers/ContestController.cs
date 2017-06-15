using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using URMade.Models;

namespace URMade.Controllers
{
    public class ContestController : Controller
    {
		ContestRepository ContestRepo {get; set;}

		public ContestController(ContestRepository contestRepo)
		{
			ContestRepo = contestRepo;
		}

		[RequirePermission(Permission.EditContests)]
		public ActionResult Index()
		{
			return View(ContestRepo.GetRecentContests(0));
		}

		[RequirePermission(Permission.EditContests)]
        [HttpGet]
		public ActionResult Create()
		{
			return View(new EditContestViewModel());
		}

		[HttpPost]
        [ValidateAntiForgeryToken]
		[RequirePermission(Permission.EditContests)]
		public ActionResult Create(EditContestViewModel model)
		{
			if (!ModelState.IsValid)
				return View(new EditContestViewModel());

			ContestRepo.CreateContestAndSave(model);
			return RedirectToAction("Index");
		}

		[RequirePermission(Permission.EditContests)]
		public ActionResult Edit(int? id)
		{
			if (id.HasValue)
				return View(new EditContestViewModel(ContestRepo.GetContest(id.Value)));
			else
				return RedirectToAction("Index");
		}

		[HttpPost]
        [ValidateAntiForgeryToken]
		[RequirePermission(Permission.EditContests)]
		public ActionResult Edit(EditContestViewModel model)
		{
			if (!ModelState.IsValid)
				return View(model);

			ContestRepo.EditContestAndSave(model);
			return RedirectToAction("Index");
		}

		[RequirePermission(Permission.EditContests)]
		public ActionResult Delete(int id)
		{
			ContestRepo.DeleteContestAndSave(id);
			return RedirectToAction("Index");
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id">Contest Entry Id</param>
        /// <returns></returns>
		[ValidateAntiForgeryToken]
        public ActionResult CastVote(int id)
        {
			if (SecurityHelper.GetLoggedInUser() == null)
				return Json(new {Success = false});

            bool success;
            int count = ContestRepo.VoteAndSave(id, out success);

            return Json(new { Success = success, Votes = count });
        }

		[HttpGet]
		public ActionResult Join(int? id)
		{
			var user = SecurityHelper.GetLoggedInUser();

			if (user == null || !id.HasValue)
				return Redirect("/");

			return View(ContestRepo.GetJoinContestViewModel(id.Value, user));
		}

		[HttpPost]
		public ActionResult Join(JoinContestViewModel model)
		{
			var user = SecurityHelper.GetLoggedInUser();

			if (user == null)
				return Redirect("/");

			if (!ModelState.IsValid)
				return View(model);

			ContestRepo.AddContestEntryAndSave(model);
			return View(ContestRepo.GetJoinContestViewModel(model.Details.ContestId, user));
		}

		[HttpGet]
		public ActionResult ViewAll(string TitleMatch)
		{
			return View(ContestRepo.GetActiveContests(TitleMatch));
		}

		[HttpGet]
		public ActionResult Vote(int? id)
		{
			if (id.HasValue)
				return View(ContestRepo.GetContestVotingPlaylist(id.Value, null));

			return RedirectToAction("ViewAll");
		}
    }
}