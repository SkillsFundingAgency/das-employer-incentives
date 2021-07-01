using System;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess
{
    public class CompletePaymentProcessInput
    {
        public DateTime CompletionDateTime { get; set; }
        public CollectionPeriod CollectionPeriod { get; set; }
    }
}
