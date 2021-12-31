using System;
using System.Collections.Generic;
using System.Text;

namespace SFA.DAS.EmployerIncentives.Commands.Services.EmploymentCheckApi
{
    public class CreateEmploymentCheckRequestDto
    {
        public CreateEmploymentCheckRequestDto(Guid correlationId, string checkType, long uln, long accountId, long apprenticeshipId, DateTime minimumDateTime, DateTime maximumDateTime)
        {
            CorrelationId = correlationId;
            CheckType = checkType;
            ULN = uln;
            AccountId = accountId;
            ApprenticeshipId = apprenticeshipId;
            MinimumDateTime = minimumDateTime;
            MaximumDateTime = maximumDateTime;
        }

        public Guid CorrelationId { get; }
        public string CheckType { get; }
        public long ULN { get; }
        public long AccountId { get; }
        public long ApprenticeshipId { get; }
        public DateTime MinimumDateTime { get; }
        public DateTime MaximumDateTime { get; }
    }
}
