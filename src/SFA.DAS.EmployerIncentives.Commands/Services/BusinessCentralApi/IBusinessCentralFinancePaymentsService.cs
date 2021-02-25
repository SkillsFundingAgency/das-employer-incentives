using SFA.DAS.EmployerIncentives.Abstractions.DTOs.Queries.ApprenticeshipIncentives;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Commands.Services.BusinessCentralApi
{
    public interface IBusinessCentralFinancePaymentsService
    {
        public int PaymentRequestsLimit { get; }
        public Task SendPaymentRequests(IList<PaymentDto> payments);
    }
}
