using System.Collections.Generic;
using System.Linq;
using SFA.DAS.EmployerIncentives.Abstractions.Domain.Services;
using SFA.DAS.EmployerIncentives.ValueObjects;

namespace SFA.DAS.EmployerIncentives.Domain.Services
{
    public class NewApprenticeIncentiveEligibilityService : INewApprenticeIncentiveEligibilityService
    {
        public IEnumerable<Apprenticeship> GetEligibileApprenticeships(IEnumerable<Apprenticeship> apprenticeships)
        {
            return apprenticeships.Where(x => NewApprenticeIncentive.NewApprenticeIncentive.IsApprenticeshipEligible(x));
        }
    }
}
