using System;
using System.Collections.Generic;
using System.Linq;
using SFA.DAS.EmployerIncentives.DataTransferObjects.Queries.ApprenticeshipIncentives;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Exceptions;
using SFA.DAS.EmployerIncentives.Enums;

namespace SFA.DAS.EmployerIncentives.Commands.Services.BusinessCentralApi
{
    public static class PaymentsDataExtensions
    {
        public static BusinessCentralFinancePaymentRequest Map(this PaymentDto payment, bool obfuscatePrivateData)
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
                PaymentLineDescription = CreatePaymentLineDescription(payment, obfuscatePrivateData),
                Approver = @"AD.HQ.DEPT\JPOOLE"
            };
        }

        public static BusinessCentralFinancePaymentRequest[] ToErrorLogOutput(this IEnumerable<BusinessCentralFinancePaymentRequest> payments)
        {
            return payments.Select(payment => new BusinessCentralFinancePaymentRequest
                {
                    RequestorUniquePaymentIdentifier = payment.RequestorUniquePaymentIdentifier,
                    DueDate = payment.DueDate,
                    AccountCode = payment.AccountCode,
                    CostCentreCode = payment.CostCentreCode,
                    ActivityCode = payment.ActivityCode
                }).ToArray();
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


        private static string CreatePaymentLineDescription(PaymentDto payment, bool obfuscateSensitiveData)
        {
            var uln = payment.ULN.ToString().ToCharArray();
            if (obfuscateSensitiveData)
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
