using System;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess
{
    public class SetSuccessfulLearnerMatchInput
    {
        public Guid ApprenticeshipIncentiveId { get; set; }
        public long Uln { get; set; }
        public bool Succeeded { get; set; }
    }
}
