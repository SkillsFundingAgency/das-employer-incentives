using System;

namespace SFA.DAS.EmployerIncentives.Api.Types
{
    public class ValidationStep
    {
        public ValidationType ValidationType { get; set; }
        public DateTime ExpiryDate { get; set; }
        public bool? Remove { get; set; }
    }
}
