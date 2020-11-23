using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Abstractions.DTOs.Queries.ApprenticeshipIncentives;

namespace SFA.DAS.EmployerIncentives.Commands.Services.BusinessCentralApi
{
    public class BusinessCentralFinancePaymentsService : IBusinessCentralFinancePaymentsService
    {
        private readonly HttpClient _client;

        public BusinessCentralFinancePaymentsService(HttpClient client)
        {
            _client = client;
        }

        public Task<PaymentsSuccessfullySent> SendPaymentRequestsForLegalEntity(List<PaymentDto> payments)
        {
            throw new System.NotImplementedException();
        }
    }
}
