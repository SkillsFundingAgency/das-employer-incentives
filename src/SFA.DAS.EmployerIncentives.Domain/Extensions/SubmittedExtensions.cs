using SFA.DAS.EmployerIncentives.Domain.IncentiveApplications.Events;
using SFA.DAS.EmployerIncentives.Domain.IncentiveApplications.Models;
using System.Collections.Generic;
using System.Linq;

namespace SFA.DAS.EmployerIncentives.Domain.Extensions
{
    public static class SubmittedExtensions
    {
        public static IEnumerable<ApprenticeshipModel> EligibleApprenticeships(this Submitted @event)
        {
            return @event.Model.ApprenticeshipModels.Where(apprenticeship =>
                !apprenticeship.WithdrawnByEmployer &&
                !apprenticeship.WithdrawnByCompliance && 
                apprenticeship.HasEligibleEmploymentStartDate);
        }
    }
}
