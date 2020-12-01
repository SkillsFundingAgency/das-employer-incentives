using System.Collections.Generic;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Abstractions.DTOs.Queries.ApprenticeshipIncentives;

namespace SFA.DAS.EmployerIncentives.Commands.Services.BusinessCentralApi
{
    public interface IBusinessCentralFinancePaymentsService
    {
        public int PaymentRequestsLimit { get; }
        public Task SendPaymentRequests(List<PaymentDto> payments);
    }
}
