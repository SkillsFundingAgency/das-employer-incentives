﻿using SFA.DAS.EmployerIncentives.Abstractions.Queries;
using SFA.DAS.EmployerIncentives.DataTransferObjects;

namespace SFA.DAS.EmployerIncentives.Queries.NewApprenticeIncentive.GetApprenticeshipEligibility
{
    public class GetApprenticeshipEligibilityRequest : IQuery
    {
        public ApprenticeshipDto Apprenticeship { get; }

        public GetApprenticeshipEligibilityRequest(ApprenticeshipDto apprenticeship)
        {
            Apprenticeship = apprenticeship;
        }
    }
}
