using System;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess
{
    public class CalculateDaysInLearningInput
    {
        public Guid ApprenticeshipIncentiveId { get; set; }
        public CollectionPeriod ActivePeriod { get; set; }        
    }
}
