using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SFA.DAS.EmployerIncentives.Abstractions.Domain;
using SFA.DAS.EmployerIncentives.Domain.IncentiveApplications;
using SFA.DAS.EmployerIncentives.Infrastructure.Configuration;

namespace SFA.DAS.EmployerIncentives.Commands.Services
{
    public class EarningsCalculationService : IEarningsCalculationService
    {
        private List<IncentivePaymentProfile> _incentivePaymentProfiles;

        public EarningsCalculationService(IOptions<ApplicationSettings> applicationSettings)
        {
            _incentivePaymentProfiles = applicationSettings.Value.IncentivePaymentProfiles;
        }

        public List<EarningCalculation> GenerateEarningsForApprenticeship(Apprenticeship apprenticeshipIncentive, IncentiveType incentiveType)
        {
            var earnings = new List<EarningCalculation>();

            if (_incentivePaymentProfiles == null)
            {
                throw new DataException("No IncentivePaymentProfiles loaded");
            }

            var incentivePaymentProfile = _incentivePaymentProfiles.FirstOrDefault(x => x.IncentiveType == incentiveType);

            if (incentivePaymentProfile?.PaymentProfiles == null)
            {
                throw new DataException($"No PaymentProfile found for incentive {incentiveType}");
            }

            foreach (var paymentProfile in incentivePaymentProfile.PaymentProfiles)
            {
                earnings.Add(new EarningCalculation
                {
                    DatePayable = apprenticeshipIncentive.PlannedStartDate.AddDays(paymentProfile.DaysAfterApprenticeshipStart),
                    AmountPayable = paymentProfile.AmountPayable
                });
            }

            return earnings;
        }
    }

    public class EarningCalculation
    {
        public DateTime DatePayable { get; set; }
        public decimal AmountPayable { get; set; }
    }
}
