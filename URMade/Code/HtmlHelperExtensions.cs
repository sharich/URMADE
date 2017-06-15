using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Web.WebPages;

namespace URMade
{
    public static class HtmlHelperExtensions
    {
        public static MvcHtmlString PropertyName<T, TResult>(
         this HtmlHelper<T> helper,
         Expression<Func<T, TResult>> property)
        {
            return MvcHtmlString.Create(property.PropertyName());
        }

        public static MvcHtmlString MakeEditableList<TModel>(this HtmlHelper<TModel> helper, string propertyName, string noun = null)
        {
            string result = @" data-editable-list=""" + propertyName + @""" ";
            if (noun != null)
            {
                result += @"data-editable-list-noun=""" + noun + @""" ";
            }

            return new MvcHtmlString(result);
        }
        public static MvcHtmlString MakeEditableListFor<TModel, TValue>(
            this HtmlHelper<TModel> helper,
            Expression<Func<TModel, TValue>> property,
            string noun = null)
        {
            return helper.MakeEditableList(propertyName: helper.PropertyName(property).ToString(), noun: noun);
        }

        public static MvcHtmlString WrappingUrl(this HtmlHelper helper, string url)
        {
            return new MvcHtmlString(helper.Encode(url).Replace(".", "&#8203;.").Replace("/", "/&#8203;"));
        }

        public static MvcHtmlString BootstrapModal(this HtmlHelper helper,
         string title,
         Func<Object, HelperResult> body,
         string id,
         string confirmLabel,
         string actionUrl = null)
        {
            string template;

            if (!String.IsNullOrWhiteSpace(actionUrl))
            {
                template = @"
      <div class=""modal fade"" 
           id=""{0}"" 
           tabindex=""-1"" 
           role=""dialog"" 
           aria-labelledby=""{0}"" 
           aria-hidden=""true"">
        <div class=""modal-dialog"">
          <div class=""modal-content"">
            <div class=""modal-header"">
              <button type=""button"" 
                      class=""close"" 
                      data-dismiss=""modal"">
                <span aria-hidden=""true"">&times;</span>
                <span class=""sr-only"">Close</span>
              </button>
              <h3 class=""modal-title"">{1}</h3>
            </div>
            <div class=""modal-body"">
              {2}
            </div>
            <div class=""modal-footer"">
              <button type=""button"" 
                      class=""btn btn-default"" 
                      data-dismiss=""modal"">
                Cancel
              </button>
              <form action=""{4}"" 
                    method=""post"" 
                    style=""display:inline-block;"">
                  <input class=""btn btn-primary"" 
                         type=""submit"" 
                         value=""{3}"">
              </form>
            </div>
          </div>
        </div>
      </div>";

                return new MvcHtmlString(
                  String.Format(
                    template,
                    id,
                    title,
                    body(null),
                    confirmLabel,
                    actionUrl));
            }
            else
            {
                template = @"
      <div class=""modal fade"" 
           id=""{0}"" 
           tabindex=""-1"" 
           role=""dialog"" 
           aria-labelledby=""{0}"" 
           aria-hidden=""true"">
        <div class=""modal-dialog"">
          <div class=""modal-content"">
            <div class=""modal-header"">
              <button type=""button"" 
                      class=""close"" 
                      data-dismiss=""modal"">
                <span aria-hidden=""true"">&times;</span>
                <span class=""sr-only"">Close</span>
              </button>
              <h3 class=""modal-title"">{1}</h3>
            </div>
            <div class=""modal-body"">
              {2}
            </div>
            <div class=""modal-footer"">
              <button type=""button"" 
                      class=""btn btn-default"" 
                      data-dismiss=""modal"">
                Cancel
              </button>
                <input class=""btn btn-primary"" 
                        type=""submit"" 
                        value=""{3}"">
            </div>
          </div>
        </div>
      </div>";

                return new MvcHtmlString(
                    String.Format(
                    template,
                    id,
                    title,
                    body(null),
                    confirmLabel));
            }
        }

        public static MvcHtmlString TriggerModal(
            this HtmlHelper helper,
            string id)
        {
            string template = @"data-toggle=""modal"" data-target=""#{0}""";
            return new MvcHtmlString(String.Format(template, id));
        }

        /// <summary>
        /// Use this method to render a template for each member of a list when you want to
        /// render the template at least once even if the list is empty. 
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="helper"></param>
        /// <param name="property"></param>
        /// <param name="fallbackValue"></param>
        /// <param name="template"></param>
        /// <returns></returns>
        public static MvcHtmlString TemplateForEach<TModel, TValue>(
            this HtmlHelper<TModel> helper,
            Expression<Func<TModel, IEnumerable<TValue>>> property,
            TValue fallbackValue,
            Func<TValue, Func<Object, HelperResult>> template)
        {
            string result = "";
            IEnumerable<TValue> list = (IEnumerable<TValue>)property.Value(helper.ViewData);

            if (list != null && list.Count() > 0)
            {
                foreach (TValue item in list)
                {
                    var templateFunc = template(item);
                    result += templateFunc(item);
                }
            }
            else
            {
                var templateFunc = template(fallbackValue);
                result += templateFunc(fallbackValue);
            }

            return new MvcHtmlString(result);
        }

        public static string DisplayShortDate(this HtmlHelper helper, DateTime? date)
        {
            if (date == null) return "";
            return date.Value.ToShortDateString();
        }

        public static string DisplayCurrency(this HtmlHelper helper, Decimal? amount)
        {
            if (amount == null) return "";
            return string.Format("{0:c}", amount.Value);
        }

        public static string DisplayInteger(this HtmlHelper helper, Decimal? number)
        {
            if (number == null) return "";
            return ((int)number.Value).ToString();
        }

        public static MvcHtmlString RadioButtonForSelectList<TModel, TProperty>(
          this HtmlHelper<TModel> htmlHelper,
          Expression<Func<TModel, TProperty>> expression,
          IEnumerable<SelectListItem> listOfValues)
        {
            var metaData = ModelMetadata.FromLambdaExpression(expression, htmlHelper.ViewData);
            var sb = new StringBuilder();

            if (listOfValues != null)
            {
                // Create a radio button for each item in the list 
                foreach (SelectListItem item in listOfValues)
                {
                    // Generate an id to be given to the radio button field 
                    var id = string.Format("{0}_{1}", metaData.PropertyName, item.Value);

                    // Create and populate a radio button using the existing html helpers 
                    var label = htmlHelper.Label(id, System.Web.HttpUtility.HtmlEncode(item.Text));
                    var radio = htmlHelper.RadioButtonFor(expression, item.Value, new { id = id }).ToHtmlString();

                    // Create the html string that will be returned to the client 
                    // e.g. <input data-val="true" data-val-required="You must select an option" id="TestRadio_1" name="TestRadio" type="radio" value="1" /><label for="TestRadio_1">Line1</label> 
                    sb.AppendFormat("<div class=\"RadioButton\">{0} {1}</div>", radio, label);
                }
            }

            return MvcHtmlString.Create(sb.ToString());
        }
    }

    public static class ExtensionMethods
    {
        /// <summary>
        /// Usage: expression.Value(htmlHelper.ViewData)
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="expression"></param>
        /// <param name="viewData"></param>
        /// <returns></returns>
        public static object Value<TModel, TProperty>(this Expression<Func<TModel, TProperty>> expression, ViewDataDictionary<TModel> viewData)
        {
            return ModelMetadata.FromLambdaExpression(expression, viewData).Model;
        }
    }
}