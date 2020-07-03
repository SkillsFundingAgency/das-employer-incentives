using SFA.DAS.EmployerIncentives.Abstractions.Queries;

namespace SFA.DAS.EmployerIncentives.Api.Controllers
{
    public class SampleQueryController : ApiQueryControllerBase
    {
        public SampleQueryController(IQueryDispatcher queryDispatcher) : base(queryDispatcher)
        {
            // HTTP GET methods only
        }
    }
}