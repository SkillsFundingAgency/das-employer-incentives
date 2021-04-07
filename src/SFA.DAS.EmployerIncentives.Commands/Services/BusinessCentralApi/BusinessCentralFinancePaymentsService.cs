using System;
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
using Microsoft.Extensions.Logging;

namespace SFA.DAS.EmployerIncentives.Commands.Services.BusinessCentralApi
{
    public class BusinessCentralFinancePaymentsService : IBusinessCentralFinancePaymentsService
    {
        private readonly HttpClient _client;
        private readonly bool _obfuscateSensitiveData;
        private readonly ILogger<BusinessCentralFinancePaymentsService> _logger;
        private readonly string _apiVersion;
        public int PaymentRequestsLimit { get; }

        public BusinessCentralFinancePaymentsService(HttpClient client, int paymentRequestsLimit, string apiVersion, bool obfuscateSensitiveData, ILogger<BusinessCentralFinancePaymentsService> logger)
        {
            _client = client;
            _obfuscateSensitiveData = obfuscateSensitiveData;
            _logger = logger;
            _apiVersion = apiVersion ?? "2020-10-01";
            PaymentRequestsLimit = paymentRequestsLimit <= 0 ? 1000 : paymentRequestsLimit;
        }

        public async Task SendPaymentRequests(IList<PaymentDto> payments)
        {
            var paymentRequests = payments.Select(MapToBusinessCentralPaymentRequest).ToList();
            
            LogNonSensitiveRequestData(paymentRequests);

            var content = CreateJsonContent(paymentRequests);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/payments-data");
            var response = await _client.PostAsync($"payments/requests?api-version={_apiVersion}", content);

            if (response.StatusCode == HttpStatusCode.Accepted)
            {
                return;
            }

            throw new BusinessCentralApiException(response.StatusCode, content);
        }

        private void LogNonSensitiveRequestData(IEnumerable<BusinessCentralFinancePaymentRequest> paymentRequests)
        {
            var nonSensitiveData = BusinessCentralPaymentsRequestLogEntry.Create(paymentRequests);
            _logger.Log(LogLevel.Information,
                "[BusinessCentralFinancePaymentsService] Sending {count} payment requests to BC {@data}",
                nonSensitiveData.Count,
                JsonConvert.SerializeObject(nonSensitiveData));
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
                DueDate = DateTime.UtcNow.ToString("yyyy-MM-dd"),
                VendorNo = payment.VendorId,
                AccountCode = MapToAccountCode(payment.SubnominalCode),
                ActivityCode = MapToActivityCode(payment.SubnominalCode),
                CostCentreCode = "10233",
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

        private static string MapToActivityCode(SubnominalCode subnominalCode)
        {
            return subnominalCode switch
            {
                SubnominalCode.Levy16To18 => "100339",
                SubnominalCode.Levy19Plus => "100388",
                SubnominalCode.NonLevy16To18 => "100349",
                SubnominalCode.NonLevy19Plus => "100397",
                _ => throw new InvalidIncentiveException($"No mapping found for SubnominalCode {subnominalCode}")
            };
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

        private static string PaymentType(EarningType earningType)
        {
            return earningType switch
            {
                EarningType.FirstPayment => "first",
                EarningType.SecondPayment => "second",
                _ => throw new InvalidIncentiveException($"No mapping found for EarningType {earningType}")
            };
        }

        private static string MapToAccountCode(SubnominalCode subnominalCode)
        {
            return subnominalCode switch
            {
                SubnominalCode.Levy16To18 => "54156003",
                SubnominalCode.Levy19Plus => "54156002",
                SubnominalCode.NonLevy16To18 => "54156003",
                SubnominalCode.NonLevy19Plus => "54156002",
                _ => throw new InvalidIncentiveException($"No mapping found for SubnominalCode {subnominalCode}")
            };
        }
    }
}
