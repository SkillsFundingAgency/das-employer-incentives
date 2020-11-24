using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SFA.DAS.EmployerIncentives.Abstractions.DTOs.Queries.ApprenticeshipIncentives;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Exceptions;
using SFA.DAS.EmployerIncentives.Enums;

namespace SFA.DAS.EmployerIncentives.Commands.Services.BusinessCentralApi
{
    public class BusinessCentralFinancePaymentsService : IBusinessCentralFinancePaymentsService
    {
        private readonly HttpClient _client;
        private string _apiVersion;
        private int _paymentRequestsLimit;

        public BusinessCentralFinancePaymentsService(HttpClient client, int paymentRequestsLimit = 1000, string apiVersion = "2020-10-01 ")
        {
            _client = client;
            _apiVersion = apiVersion;
            _paymentRequestsLimit = paymentRequestsLimit;
        }

        public async Task<PaymentsSuccessfullySent> SendPaymentRequestsForLegalEntity(List<PaymentDto> payments)
        {
            var paymentsToSend = payments.Take(_paymentRequestsLimit).ToList();

            var paymentRequests = paymentsToSend.Select(MapToBusinessCentralPaymentRequest);
            var body = new PaymentRequestContainer { PaymentRequests = paymentRequests.ToArray()};
            var content = new StringContent(JsonConvert.SerializeObject(body), Encoding.Default);
            var response = await _client.PostAsync($"payments/requests?api-version={_apiVersion}", content);

            if (response.StatusCode == HttpStatusCode.Accepted)
            {
                var morePaymentsToSend = payments.Count > paymentsToSend.Count;
                return new PaymentsSuccessfullySent(paymentsToSend, !morePaymentsToSend);
            }

            if (response.StatusCode >= HttpStatusCode.InternalServerError)
            {
                throw new BusinessCentralApiException($"Business Central API is unavailable and returned an internal code of {response.StatusCode}", response.StatusCode);
            }

            throw new BusinessCentralApiException($"Business Central API returned a server code of {response.StatusCode}", response.StatusCode);
        }

        public BusinessCentralFinancePaymentRequest MapToBusinessCentralPaymentRequest(PaymentDto payment)
        {
            return new BusinessCentralFinancePaymentRequest
            {
                RequestorUniquePaymentIdentifier = payment.PaymentId,
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

        private string CreatePaymentLineDescription(PaymentDto payment)
        {
            return $"Hire a new apprentice ({payment.PaymentSequence} payment). Employer: {payment.HashedLegalEntityId} ULN: {payment.ULN}";
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
