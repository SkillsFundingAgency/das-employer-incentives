using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Commands.Services.BusinessCentralApi
{
    public interface IBusinessCentralFinancePaymentsService
    {
        public Task<PaymentsSuccessfullySent> SendPaymentRequestsForLegalEntity(List<Domain.ApprenticeshipIncentives.ApprenticeshipIncentive> apprenticeshipIncentives);
    }
}
