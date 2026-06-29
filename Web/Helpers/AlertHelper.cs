using Microsoft.AspNetCore.Mvc;

namespace Web.Helpers
{
    public static class AlertHelper
    {
        public const string SuccessKey = "SuccessMessage";
        public const string ErrorKey = "ErrorMessage";
        public const string WarningKey = "WarningMessage";

        public static void AddSuccess(this Controller controller, string message)
        {
            controller.TempData[SuccessKey] = message;
        }

        public static void AddError(this Controller controller, string message)
        {
            controller.TempData[ErrorKey] = message;
        }

        public static void AddWarning(this Controller controller, string message)
        {
            controller.TempData[WarningKey] = message;
        }
    }
}