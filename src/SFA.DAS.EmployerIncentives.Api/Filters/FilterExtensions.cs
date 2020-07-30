using Microsoft.AspNetCore.Mvc.Filters;

namespace SFA.DAS.EmployerIncentives.Api.Filters
{
    public static class FilterExtensions
    {
        public static void AddUnitOfWorkFilter(this FilterCollection filters)
        {
            filters.Add(new UnitOfWorkManagerFilterAttribute());
        }
    }
}
