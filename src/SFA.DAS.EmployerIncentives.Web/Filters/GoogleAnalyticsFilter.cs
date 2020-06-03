using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SFA.DAS.EmployerIncentives.Web.Infrastructure;
using SFA.DAS.EmployerIncentives.Web.Models;

namespace SFA.DAS.EmployerIncentives.Web.Filters
{
    public class GoogleAnalyticsFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!(context.Controller is Controller controller))
            {
                return;
            }

            controller.ViewBag.GaData = PopulateForEmployer(context);

            base.OnActionExecuting(context);
        }

        private GoogleAnalyticsData PopulateForEmployer(ActionExecutingContext context)
        {
            string hashedAccountId = null;

            var userId = context.HttpContext.User.Claims.FirstOrDefault(c => c.Type.Equals(EmployerClaims.IdamsUserIdClaimTypeIdentifier))?.Value;

            if (context.RouteData.Values.TryGetValue("employerAccountId", out var employerAccountId))
            {
                hashedAccountId = employerAccountId.ToString();
            }

            return new GoogleAnalyticsData
            {
                UserId = userId,
                Acc = hashedAccountId
            };
        }

        public string DataLoaded { get; set; }
    }
}