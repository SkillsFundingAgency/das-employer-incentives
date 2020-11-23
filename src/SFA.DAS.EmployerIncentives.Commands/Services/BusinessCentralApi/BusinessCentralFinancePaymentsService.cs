using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Commands.Services.BusinessCentralApi
{
    public class BusinessCentralFinancePaymentsService : IBusinessCentralFinancePaymentsService
    {
        private readonly HttpClient _client;

        public BusinessCentralFinancePaymentsService(HttpClient client)
        {
            _client = client;
        }

        Task<PaymentsSuccessfullySent> IBusinessCentralFinancePaymentsService.SendPaymentRequestsForLegalEntity(List<Domain.ApprenticeshipIncentives.ApprenticeshipIncentive> apprenticeshipIncentives)
        {
            throw new System.NotImplementedException();
        }
    }
}
