using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace URMade.Models
{
    public class SelectOptionIndexViewModel
    {
        public List<SelectOptionGroupViewModel> OptionGroups { get; set; }

        public SelectOptionIndexViewModel()
        {
            OptionGroups = new List<SelectOptionGroupViewModel>();
        }
    }

    public class SelectOptionGroupViewModel
    {
        public string UrlId { get; set; }
        public string OptionGroupName { get; set; }
        public List<SelectOption> SelectOptions { get; set; }

        public SelectOptionGroupViewModel() { }

        public SelectOptionGroupViewModel(string name, List<SelectOption> options)
        {
            UrlId = DataHelper.EncodeUrlIdFromString(name);
            OptionGroupName = name;
            if (options != null && options.Count() > 0)
            {
                SelectOptions = options.OrderBy(p => p.SortOrder).ToList();
            }
            else
            {
                SelectOptions = new List<SelectOption>();
            }
        }
    }

    public class EditSelectOptionGroupViewModel
    {
        public string OldOptionGroupName { get; set; }
        public string NewOptionGroupName { get; set; }

        public List<SelectOption> SelectOptions { get; set; }

        public EditSelectOptionGroupViewModel()
        {
            SelectOptions = new List<SelectOption>();
        }

        public EditSelectOptionGroupViewModel(string name, List<SelectOption> options)
        {
            OldOptionGroupName = name;
            NewOptionGroupName = name;
            if (options != null && options.Count() > 0)
            {
                SelectOptions = options.OrderBy(p => p.SortOrder).ToList();
            }
            else
            {
                SelectOptions = new List<SelectOption>();
            }
        }
    }
}