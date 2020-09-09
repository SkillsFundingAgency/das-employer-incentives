using SFA.DAS.Common.Domain.Types;
using SFA.DAS.EmployerIncentives.Domain.IncentiveApplications;
using SFA.DAS.EmployerIncentives.Domain.IncentiveApplications.Models;
using System;
using System.Collections.Generic;
using SFA.DAS.EmployerIncentives.Domain.ValueObjects;

namespace SFA.DAS.EmployerIncentives.Domain.Factories
{
    public class IncentiveApplicationFactory : IIncentiveApplicationFactory
    {
        public IncentiveApplication CreateNew(Guid id, long accountId, long accountLegalEntityId)
        {
            return IncentiveApplication.New(id, accountId, accountLegalEntityId);
        }

        public Apprenticeship CreateApprenticeship(long apprenticeshipId, string firstName, string lastName, DateTime dateOfBirth, long uln, DateTime plannedStartDate, 
            ApprenticeshipEmployerType apprenticeshipEmployerTypeOnApproval, List<IncentivePaymentProfile> incentivePaymentProfiles)
        {
            return new Apprenticeship(Guid.NewGuid(), apprenticeshipId, firstName, lastName, dateOfBirth, uln, plannedStartDate, apprenticeshipEmployerTypeOnApproval, incentivePaymentProfiles);
        }

        public IncentiveApplication GetExisting(Guid id, IncentiveApplicationModel model)
        {
            return IncentiveApplication.Get(id, model);
        }
    }
}
