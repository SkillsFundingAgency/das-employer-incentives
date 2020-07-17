using System.Collections.Generic;
using SFA.DAS.EmployerIncentives.ValueObjects;

namespace SFA.DAS.EmployerIncentives.Abstractions.Domain.Services
{
    public interface INewApprenticeIncentiveEligibilityService
    {
        IEnumerable<Apprenticeship> GetEligibileApprenticeships(IEnumerable<Apprenticeship> apprenticeships);
    }
}
