using SFA.DAS.Common.Domain.Types;
using SFA.DAS.EmployerIncentives.Domain.IncentiveApplications;
using SFA.DAS.EmployerIncentives.Domain.IncentiveApplications.Models;
using System;
using System.Collections.Generic;
using SFA.DAS.EmployerIncentives.Domain.ValueObjects;

namespace SFA.DAS.EmployerIncentives.Domain.Factories
{
    public interface IIncentiveApplicationFactory
    {
        IncentiveApplication CreateNew(Guid id, long accountId, long accountLegalEntityId);
        Apprenticeship CreateApprenticeship(long apprenticeshipId, string firstName, string lastName, DateTime dateOfBirth, long uln, DateTime plannedStartDate, 
            ApprenticeshipEmployerType apprenticeshipEmployerTypeOnApproval, List<IncentivePaymentProfile> incentivePaymentProfiles);
        IncentiveApplication GetExisting(Guid id, IncentiveApplicationModel model);
    }
}
