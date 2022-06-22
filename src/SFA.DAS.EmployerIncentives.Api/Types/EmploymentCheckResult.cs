using System;

namespace SFA.DAS.EmployerIncentives.Api.Types
{
    public class EmploymentCheckResult
    {
        public Guid CorrelationId { get; set; }
        public string Result { get; set; }
        public DateTime DateChecked { get; set; }
    }
}
