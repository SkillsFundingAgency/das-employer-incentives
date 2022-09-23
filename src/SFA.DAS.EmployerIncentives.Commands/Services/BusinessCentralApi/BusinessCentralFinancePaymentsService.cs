using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using SFA.DAS.EmployerIncentives.DataTransferObjects.Queries.ApprenticeshipIncentives;
using SFA.DAS.EmployerIncentives.Infrastructure.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Commands.Services.BusinessCentralApi
{
    public class BusinessCentralFinancePaymentsService : IBusinessCentralFinancePaymentsService
    {
        private readonly HttpClient _client;
        private readonly bool _obfuscateSensitiveData;
        private readonly string _apiVersion;
        public int PaymentRequestsLimit { get; }

        public BusinessCentralFinancePaymentsService(
            HttpClient client,
            IOptions<BusinessCentralApiClient> options)
        {
            _client = client;
            var config = options.Value;
            _obfuscateSensitiveData = config.ObfuscateSensitiveData;
            _apiVersion = config.ApiVersion ?? "2020-10-01";
            PaymentRequestsLimit = config.PaymentRequestsLimit <= 0 ? 1000 : config.PaymentRequestsLimit;
        }

        public async Task SendPaymentRequests(IList<Payment> payments)
        {
            var paymentRequests = payments.Select(x => x.Map(_obfuscateSensitiveData)).ToList();

            var content = CreateJsonContent(paymentRequests);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/payments-data");
            var response = await _client.PostAsync($"payments/requests?api-version={_apiVersion}", content);

            if (response.StatusCode == HttpStatusCode.Accepted)
            {
                return;
            }

            throw new BusinessCentralApiException(response.StatusCode, CreateErrorLogJsonContent(paymentRequests));
        }

        private static HttpContent CreateJsonContent(IEnumerable<BusinessCentralFinancePaymentRequest> payments)
        {
            var body = new PaymentRequestContainer { PaymentRequests = payments.ToArray() };
            var jsonSerializerSettings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
            return new StringContent(JsonConvert.SerializeObject(body, jsonSerializerSettings), Encoding.Default, "application/json");
        }

        private static HttpContent CreateErrorLogJsonContent(IEnumerable<BusinessCentralFinancePaymentRequest> payments)
        {
            var body = new PaymentRequestContainer { PaymentRequests = payments.ToErrorLogOutput() };
            var jsonSerializerSettings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
            return new StringContent(JsonConvert.SerializeObject(body, jsonSerializerSettings), Encoding.Default, "application/json");
        }
    }
}
