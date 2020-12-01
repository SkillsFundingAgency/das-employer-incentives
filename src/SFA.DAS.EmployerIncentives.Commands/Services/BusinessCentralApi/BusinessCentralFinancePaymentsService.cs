using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using SFA.DAS.EmployerIncentives.Abstractions.DTOs.Queries.ApprenticeshipIncentives;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Exceptions;
using SFA.DAS.EmployerIncentives.Enums;

namespace SFA.DAS.EmployerIncentives.Commands.Services.BusinessCentralApi
{
    public class BusinessCentralFinancePaymentsService : IBusinessCentralFinancePaymentsService
    {
        private readonly HttpClient _client;
        private string _apiVersion;
        public int PaymentRequestsLimit { get; }

        public BusinessCentralFinancePaymentsService(HttpClient client, int paymentRequestsLimit, string apiVersion)
        {
            _client = client;
            _apiVersion = apiVersion ?? "2020-10-01";
            PaymentRequestsLimit = paymentRequestsLimit <= 0 ? 1000 : paymentRequestsLimit;
        }

        public async Task SendPaymentRequests(List<PaymentDto> payments)
        {
            var content = CreateJsonContent(payments);
            var response = await _client.PostAsync($"payments/requests?api-version={_apiVersion}", content);

            if (response.StatusCode == HttpStatusCode.Accepted)
            {
                return;
            }

            throw new BusinessCentralApiException(response.StatusCode);
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
                    StartDate = new DateTime(2020, 9, 1),
                    EndDate = new DateTime(2021, 8, 30),
                },
                DueDate = payment.DueDate,
                VendorNo = payment.VendorId,
                AccountCode = MapToAccountCode(payment.SubnominalCode),
                CostCentreCode = "AAA40",
                Amount = payment.Amount,
                Currency = "GBP",
                ExternalReference = new ExternalReference
                {
                    Type = "ApprenticeIdentifier",
                    Value = payment.AccountLegalEntityId.ToString()
                },
                PaymentLineDescription = CreatePaymentLineDescription(payment),
                Approver = @"AD.HQ.DEPT\JPOOLE"
            };
        }

        private HttpContent CreateJsonContent(List<PaymentDto> paymentsToSend)
        {
            var paymentRequests = paymentsToSend.Select(MapToBusinessCentralPaymentRequest);

            var body = new PaymentRequestContainer { PaymentRequests = paymentRequests.ToArray() };
            var jsonSerializerSettings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
            return new StringContent(JsonConvert.SerializeObject(body, jsonSerializerSettings), Encoding.Default);
        }

        private string CreatePaymentLineDescription(PaymentDto payment)
        {
            return $"Hire a new apprentice ({PaymentType(payment.EarningType)} payment). Employer: {payment.HashedLegalEntityId} ULN: {payment.ULN}";
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
