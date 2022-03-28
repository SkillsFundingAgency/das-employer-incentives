using System;

namespace SFA.DAS.EmployerIncentives.Commands.Services.EmploymentCheckApi
{
    public class RegisterCheckRequest
    {
        public Guid CorrelationId { get; set; }
        public string CheckType { get; set; }
        public long Uln { get; set; }
        public long ApprenticeshipAccountId { get; set; }
        public long? ApprenticeshipId { get; set; }
        public DateTime MinDate { get; set; }
        public DateTime MaxDate { get; set; }
    }
}
