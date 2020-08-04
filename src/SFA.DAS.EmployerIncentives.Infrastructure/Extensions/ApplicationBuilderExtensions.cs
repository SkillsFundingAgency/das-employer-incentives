using Microsoft.AspNetCore.Builder;
using SFA.DAS.EmployerIncentives.Infrastructure.UnitOfWork;

namespace SFA.DAS.EmployerIncentives.Infrastructure.Extensions
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseUnitOfWork(this IApplicationBuilder app)
        {
            return app.UseMiddleware<UnitOfWorkManagerMiddleware>();
        }
    }
}
