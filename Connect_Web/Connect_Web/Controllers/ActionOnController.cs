using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Connect_Web.Controllers
{
    public class ActionOnControllerAttribute : ActionFilterAttribute, IActionFilter
    {
        private DateTime _startTime;
        void IActionFilter.OnActionExecuting(ActionExecutingContext filterContext)
        {
            _startTime = DateTime.Now;

            if (filterContext.Controller is Controller controller)
            {
                // Accès à ViewBag via le cast en Controller
                controller.ViewBag.suggeredUrls = (filterContext.Controller as BaseController)?.SuggeredUrls;
            }
        }

        void IActionFilter.OnActionExecuted(ActionExecutedContext filterContext)
        {
            if (filterContext.Controller is Controller controller)
            {
                // Accès à ViewBag via le cast en Controller
                controller.ViewBag.ApiUrl = (filterContext.Controller as BaseController)?.ApiUrl;
            }
        }
    }
}