using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using SFA.DAS.EmployerIncentives.Abstractions.DTOs.Queries.ApprenticeshipIncentives;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Exceptions;
using SFA.DAS.EmployerIncentives.Enums;
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
        private readonly ILogger<BusinessCentralFinancePaymentsService> _logger;
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
            var content = CreateJsonContent(payments);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/payments-data");
            var response = await _client.PostAsync($"payments/requests?api-version={_apiVersion}", content);

            if (response.StatusCode == HttpStatusCode.Accepted)
            {
                return;
            }

            throw new BusinessCentralApiException(response.StatusCode, content);
        }

        public BusinessCentralFinancePaymentRequest MapToBusinessCentralPaymentRequest(PaymentDto payment)
        {
            return new BusinessCentralFinancePaymentRequest
            {
                RequestorUniquePaymentIdentifier = payment.PaymentId.ToString("N"),
                Requestor = "ApprenticeServiceEI",
                FundingStream = new FundingStream
                {
                    Code = "EIAPP",
                    StartDate = "2020-09-01",
                    EndDate = "2021-08-30",
                },
                DueDate = payment.DueDate.ToString("yyyy-MM-dd"),
                VendorNo = payment.VendorId,
                AccountCode = MapToAccountCode(payment.SubnominalCode),
                CostCentreCode = "AAA40",
                Amount = payment.Amount,
                Currency = "GBP",
                ExternalReference = new ExternalReference
                {
                    Type = "ApprenticeIdentifier",
                    Value = payment.HashedLegalEntityId
                },
                PaymentLineDescription = CreatePaymentLineDescription(payment),
                Approver = @"AD.HQ.DEPT\JPOOLE"
            };
        }

        private HttpContent CreateJsonContent(IEnumerable<PaymentDto> paymentsToSend)
        {
            var paymentRequests = paymentsToSend.Select(MapToBusinessCentralPaymentRequest);

            var body = new PaymentRequestContainer { PaymentRequests = paymentRequests.ToArray() };
            var jsonSerializerSettings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
            return new StringContent(JsonConvert.SerializeObject(body, jsonSerializerSettings), Encoding.Default, "application/json");
        }

        private string CreatePaymentLineDescription(PaymentDto payment)
        {
            var uln = payment.ULN.ToString().ToCharArray();
            if (_obfuscateSensitiveData)
            {
                for (var i = 0; i < uln.Length - 4; i++)
                {
                    uln[i] = '*';
                }
            }

            return $"Hire a new apprentice ({PaymentType(payment.EarningType)} payment). Employer: {payment.HashedLegalEntityId} ULN: {new string(uln)}";
        }

        private string PaymentType(EarningType earningType)
        {
            switch (earningType)
            {
                case EarningType.FirstPayment:
                    return "first";
                case EarningType.SecondPayment:
                    return "second";
                default:
                    throw new InvalidIncentiveException($"No mapping found for EarningType {earningType}");
            }
        }

        private string MapToAccountCode(SubnominalCode subnominalCode)
        {
            switch (subnominalCode)
            {
                case SubnominalCode.Levy16To18:
                    return "2240147";
                case SubnominalCode.Levy19Plus:
                    return "2340147";
                case SubnominalCode.NonLevy16To18:
                    return "2240250";
                case SubnominalCode.NonLevy19Plus:
                    return "2340292";
                default:
                    throw new InvalidIncentiveException($"No mapping found for subnominalCode {subnominalCode}");
            }
        }
    }
}
