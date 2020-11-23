using System.Collections.Generic;

namespace SFA.DAS.EmployerIncentives.Commands.Services.BusinessCentralApi
{
    public class PaymentsSuccessfullySent
    {
        public PaymentsSuccessfullySent(List<Domain.ApprenticeshipIncentives.ApprenticeshipIncentive> apprenticeshipIncentives, bool allPaymentsSent)
        {
            ApprenticeshipIncentives = apprenticeshipIncentives;
            AllPaymentsSent = allPaymentsSent;
        }
        public List<Domain.ApprenticeshipIncentives.ApprenticeshipIncentive> ApprenticeshipIncentives { get; }
        public bool AllPaymentsSent { get; }
    }
}