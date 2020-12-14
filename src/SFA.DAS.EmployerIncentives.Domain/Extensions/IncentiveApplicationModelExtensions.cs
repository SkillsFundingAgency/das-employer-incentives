using SFA.DAS.EmployerIncentives.Domain.IncentiveApplications.Models;
using System.Collections.Generic;
using System.Linq;

namespace SFA.DAS.EmployerIncentives.Domain.Extensions
{
    public static class IncentiveApplicationModelExtensions
    {
        public static IEnumerable<ApprenticeshipModel> EligibleApprenticeships(this IncentiveApplicationModel model)
        {
            return model.ApprenticeshipModels.Where(a => !a.WithdrawnByEmployer);
        }
    }
}
