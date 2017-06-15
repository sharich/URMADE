using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using URMade.Models;

namespace URMade
{
    [Authorize]
    [RequirePermission(Permission.EditSelectOptions)]
    public class SelectOptionsController : Controller
    {
        public SelectOptionRepository OptionRepo { get; set; }

        public SelectOptionsController(SelectOptionRepository optionRepo)
        {
            OptionRepo = optionRepo;
        }

        // GET: SelectOptions
        public ActionResult Index()
        {
            SelectOptionIndexViewModel model = OptionRepo.GetSelectOptionIndexViewModel();
            return View(model);
        }

        [HttpGet]
        public ActionResult Edit(string id)
        {
            EditSelectOptionGroupViewModel model = OptionRepo.GetEditSelectOptionGroupViewModel(id);
            return View(model);
        }

        [HttpPost]
        public ActionResult Edit(string id, EditSelectOptionGroupViewModel model)
        {
            if (ModelState.IsValid)
            {
                OptionRepo.UpdateSelectOptionGroupAndSave(model);
                this.FlashSuccessNotification(String.Format("OptionGroup {0} has been updated.", model.NewOptionGroupName));
                return RedirectToAction("Index");
            }

            return View(model);
        }
    }
}