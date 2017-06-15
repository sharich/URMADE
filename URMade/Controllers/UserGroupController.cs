using URMade.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace URMade.Controllers
{
    [Authorize]
    [RequirePermission(Permission.EditUserGroups)]
    public class UserGroupController : Controller
    {
        public UserGroupRepository UserGroupRepo { get; set; }

        public UserGroupController(UserGroupRepository userGroupRepo)
        {
            UserGroupRepo = userGroupRepo;
        }

        // GET: UserGroup
        public ActionResult Index()
        {
            UserGroupIndexViewModel model = UserGroupRepo.GetUserGroupIndexViewModel();

            return View(model);
        }

        public ActionResult Details(int id)
        {
            UserGroupViewModel model = UserGroupRepo.GetUserGroupViewModel(id);
            if (model == null) return HttpNotFound();

            return View(model);
        }

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            bool deleted = UserGroupRepo.DeleteUserGroupAndSave(id);
            if (deleted)
            {
                this.FlashSuccessNotification("User Group has been successfully deleted.");
            }
            else
            {
                this.FlashErrorNotification("There was a problem deleting this user group");
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult Add()
        {
            EditUserGroupViewModel model = UserGroupRepo.GetBlankEditUserGroupViewModel();
            return View(model);
        }

        [HttpPost]
        public ActionResult Add(EditUserGroupViewModel model)
        {
            if (ModelState.IsValid)
            {
                UserGroup group = UserGroupRepo.CreateUserGroupAndSave(model);
                return RedirectToAction("Details", new { id = group.UserGroupId });
            }

            UserGroupRepo.RefreshEditUserGroupViewModel(model);
            return View(model);
        }

        [HttpGet]
        public ActionResult Edit(int id)
        {
            EditUserGroupViewModel model = UserGroupRepo.GetEditUserGroupViewModel(id);
            if (model == null) return HttpNotFound();

            return View(model);
        }

        [HttpPost]
        public ActionResult Edit(int id, EditUserGroupViewModel model)
        {
            if (UserGroupRepo.Exists(id) == false) return HttpNotFound();

            if (ModelState.IsValid)
            {
                UserGroupRepo.UpdateUserGroupAndSave(model);
                return RedirectToAction("Details", new { id = id });
            }

            return View();
        }

		[RequirePermission(Permission.EditUserGroups)]
		public ActionResult ResetUserGroupPermissions()
		{
			UserGroupRepo.ResetPermissionsAndSave();
			return Redirect("/");
		}
    }
}