using SFA.DAS.EmployerIncentives.Enums;
using System;

namespace SFA.DAS.EmployerIncentives.Api.Types
{
    public class UpdateEmploymentCheckRequest
    {
        public Guid CorrelationId { get; set; }
        public EmploymentCheckResultType Result { get; set; }
        public DateTime DateChecked { get; set; }
    }
}
