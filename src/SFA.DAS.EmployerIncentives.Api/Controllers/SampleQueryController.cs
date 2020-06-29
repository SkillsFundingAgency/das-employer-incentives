using SFA.DAS.EmployerIncentives.Queries;

namespace SFA.DAS.EmployerIncentives.Api.Controllers
{
    public class SampleQueryController : ApiQueryControllerBase
    {
        public SampleQueryController(IQueryProvider queryProvider) : base(queryProvider)
        {
            // HTTP GET methods only
        }
    }
}