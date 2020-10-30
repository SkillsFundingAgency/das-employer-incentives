using Microsoft.AspNetCore.Mvc;
using SFA.DAS.EmployerIncentives.Abstractions.Queries;

namespace SFA.DAS.EmployerIncentives.Api.Controllers
{
    [ApiController]
    public class EarningsResilienceQueryController : ApiQueryControllerBase
    {
        public EarningsResilienceQueryController(IQueryDispatcher queryDispatcher) : base(queryDispatcher)
        {
        }
    }
}