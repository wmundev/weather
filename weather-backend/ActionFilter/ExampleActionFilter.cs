using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace weather_backend.ActionFilter
{
    public class ExampleActionFilter : IActionFilter
    {
        // do something before the action executes
        public void OnActionExecuting(ActionExecutingContext context)
        {
            context.Result = new BadRequestObjectResult(context.ModelState);
        }

        // do something after the action executes
        public void OnActionExecuted(ActionExecutedContext context)
        {
            if (context.Canceled) throw new Exception("sad");
        }
    }
}