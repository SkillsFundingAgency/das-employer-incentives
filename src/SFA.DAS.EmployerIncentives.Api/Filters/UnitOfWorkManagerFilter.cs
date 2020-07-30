using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using SFA.DAS.UnitOfWork.Managers;

namespace SFA.DAS.EmployerIncentives.Api.Filters
{
    public class UnitOfWorkManagerFilterAttribute : ActionFilterAttribute
    {      
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            context.HttpContext.RequestServices.GetService<IUnitOfWorkManager>().BeginAsync().GetAwaiter().GetResult();
        }

        public override void OnActionExecuted(ActionExecutedContext context)
        {
            context.HttpContext.RequestServices.GetService<IUnitOfWorkManager>().EndAsync(context.Exception).GetAwaiter().GetResult();
        }
    }
}
