using URMade.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace URMade
{
    public class SelectOptionRepository
    {
        private ApplicationDbContext Context { get; set; }

        public SelectOptionRepository(ApplicationDbContext context)
        {
            Context = context;
        }

        public SelectOptionIndexViewModel GetSelectOptionIndexViewModel()
        {
            var model = new SelectOptionIndexViewModel();

            List<SelectOption> allOptions = Context.SelectOptions.ToList();
            if (allOptions != null && allOptions.Count() > 0)
            {
                model.OptionGroups = allOptions
                                .GroupBy(p => p.OptionGroup)
                                .Select(p => new SelectOptionGroupViewModel(p.Key, p.ToList()))
                                .ToList();
            }

            return model;
        }

        public SelectOptionGroupViewModel GetSelectOptionGroupViewModel(string encodedId)
        {
            string groupName = DataHelper.DecodeStringFromUrlId(encodedId);
            var existingOptions = Context.SelectOptions.Where(p => p.OptionGroup == groupName).ToList();
            if (existingOptions == null || existingOptions.Count() == 0) return null;

            return new SelectOptionGroupViewModel(groupName, existingOptions);
        }

        public EditSelectOptionGroupViewModel GetEditSelectOptionGroupViewModel(string encodedId)
        {
            string groupName = DataHelper.DecodeStringFromUrlId(encodedId);
            var options = new List<SelectOption>();

            var existingOptions = Context.SelectOptions.Where(p => p.OptionGroup == groupName).ToList();
            if (existingOptions != null && existingOptions.Count() > 0) options = existingOptions;

            return new EditSelectOptionGroupViewModel(groupName, options);
        }

        public void UpdateSelectOptionGroupAndSave(EditSelectOptionGroupViewModel model)
        {
            string groupName = model.OldOptionGroupName;
            var oldOptions = Context.SelectOptions.Where(p => p.OptionGroup == groupName);
            var newOptions = new List<SelectOption>();

            for (var i = 0; i < model.SelectOptions.Count(); i++)
            {
                var option = new SelectOption()
                {
                    OptionGroup = model.NewOptionGroupName,
                    SortOrder = i,
                    Text = model.SelectOptions[i].Text,
                    Value = model.SelectOptions[i].Value
                };

                newOptions.Add(option);
            }

            Context.SelectOptions.AddRange(newOptions);
            Context.SelectOptions.RemoveRange(oldOptions);

            Context.SaveChanges();
        }
    }
}