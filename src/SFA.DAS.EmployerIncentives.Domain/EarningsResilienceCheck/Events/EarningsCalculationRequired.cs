using SFA.DAS.EmployerIncentives.Abstractions.Logging;
using SFA.DAS.EmployerIncentives.Domain.IncentiveApplications;
using System;

namespace SFA.DAS.EmployerIncentives.Domain.EarningsResilienceCheck.Events
{
    public class EarningsCalculationRequired : IDomainEvent, ILogWriter
    {
        public long AccountId { get; set; }
        public Guid IncentiveApplicationId { get; set; }

        public Log Log
        {
            get
            {
                var message = $"Earnings Calculation Required event with AccountId {AccountId} and IncentiveApplicationId {IncentiveApplicationId}";
                return new Log
                {
                    OnProcessing = () => message,
                    OnError = () => message
                };
            }
        }
    }
}
