using System;

namespace SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes
{
    public class ValidationOverrideStep
    {
        public string ValidationType { get; set; }
        public DateTime ExpiryDate { get; set; }
    }
}
