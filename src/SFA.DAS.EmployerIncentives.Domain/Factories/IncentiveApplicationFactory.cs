using System;
using SFA.DAS.Common.Domain.Types;
using SFA.DAS.EmployerIncentives.Domain.IncentiveApplications;
using SFA.DAS.EmployerIncentives.Domain.IncentiveApplications.Models;

namespace SFA.DAS.EmployerIncentives.Domain.Factories
{
    public class IncentiveApplicationFactory : IIncentiveApplicationFactory
    {
        public IncentiveApplication CreateNew(Guid id, long accountId, long accountLegalEntityId)
        {
            return IncentiveApplication.New(id, accountId, accountLegalEntityId);
        }

        public Apprenticeship CreateNewApprenticeship(Guid id, int apprenticeshipId, string firstName, string lastName, DateTime dateOfBirth, long uln, DateTime plannedStartDate, ApprenticeshipEmployerType apprenticeshipEmployerTypeOnApproval)
        {
            var model = new ApprenticeshipModel
            {
                Id = id,
                ApprenticeshipId = apprenticeshipId,
                FirstName = firstName,
                LastName = lastName,
                DateOfBirth = dateOfBirth,
                Uln = uln,
                PlannedStartDate = plannedStartDate,
                ApprenticeshipEmployerTypeOnApproval = apprenticeshipEmployerTypeOnApproval
            };
            return Apprenticeship.Create(model);
        }
    }
}
