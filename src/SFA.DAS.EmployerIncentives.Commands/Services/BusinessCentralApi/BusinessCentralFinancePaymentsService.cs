using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using SFA.DAS.EmployerIncentives.Abstractions.DTOs.Queries.ApprenticeshipIncentives;
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

        public BusinessCentralFinancePaymentsService(HttpClient client, int paymentRequestsLimit, string apiVersion, bool obfuscateSensitiveData)
        {
            _client = client;
            _obfuscateSensitiveData = obfuscateSensitiveData;
            _apiVersion = apiVersion ?? "2020-10-01";
            PaymentRequestsLimit = paymentRequestsLimit <= 0 ? 1000 : paymentRequestsLimit;
        }

        public async Task SendPaymentRequests(IList<PaymentDto> payments)
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
            var body = new PaymentRequestContainer {PaymentRequests = payments.ToErrorLogOutput()};
            var jsonSerializerSettings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
            return new StringContent(JsonConvert.SerializeObject(body, jsonSerializerSettings), Encoding.Default, "application/json");
        }
    }
}
