using System.Collections.Generic;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.DataTransferObjects.Queries.ApprenticeshipIncentives;

namespace SFA.DAS.EmployerIncentives.Commands.Services.BusinessCentralApi
{
    public interface IBusinessCentralFinancePaymentsService
    {
        public int PaymentRequestsLimit { get; }
        public Task SendPaymentRequests(IList<Payment> payments);
    }
}
