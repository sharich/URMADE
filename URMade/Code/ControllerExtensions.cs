using System.Linq;
using System.Web.Mvc;

namespace URMade
{
    public class ActionNotification
    {
        public NotificationType Type { get; set; }
        public string Message { get; set; }
    }

    public enum NotificationType
    {
        Error,
        Success
    }

    public static class ControllerExtensions
    {
        /// <summary>
        /// Use this to display a success notification on a page you redirect to.
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="message"></param>
        public static void FlashSuccessNotification(this Controller controller, string message)
        {
            controller.TempData["FlashNotification"] = new ActionNotification
            {
                Type = NotificationType.Success,
                Message = message
            };
        }

        /// <summary>
        /// Use this to display an error notification on a page you redirect to.
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="message"></param>
        public static void FlashErrorNotification(this Controller controller, string message)
        {
            controller.TempData["FlashNotification"] = new ActionNotification
            {
                Type = NotificationType.Error,
                Message = message
            };
        }

        public static void AddSuccessNotification(this Controller controller, string message)
        {
            controller.ViewBag.notification = new ActionNotification
            {
                Type = NotificationType.Success,
                Message = message
            };
        }

        public static void AddErrorNotification(this Controller controller, string message)
        {
            controller.ViewBag.notification = new ActionNotification
            {
                Type = NotificationType.Error,
                Message = message
            };
        }

        public static string FirstErrorMessage(this ModelStateDictionary modelState)
        {
            return modelState
                    .SelectMany(p => p.Value.Errors)
                    .Select(p => p.ErrorMessage)
                    .First();
        }
    }
}