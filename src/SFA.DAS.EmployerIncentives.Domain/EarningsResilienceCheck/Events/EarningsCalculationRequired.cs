using SFA.DAS.EmployerIncentives.Abstractions.Logging;
using SFA.DAS.EmployerIncentives.Domain.IncentiveApplications;
using SFA.DAS.EmployerIncentives.Domain.IncentiveApplications.Models;

namespace SFA.DAS.EmployerIncentives.Domain.EarningsResilienceCheck.Events
{
    public class EarningsCalculationRequired : IDomainEvent, ILogWriter
    {
        public IncentiveApplicationModel Model { get; }

        public EarningsCalculationRequired(IncentiveApplicationModel model)
        {
            Model = model;
        }

        public Log Log
        {
            get
            {
                var message = $"Earnings Calculation Required event with AccountId {Model.AccountId} and IncentiveApplicationId {Model.Id}";
                return new Log
                {
                    OnProcessing = () => message,
                    OnError = () => message
                };
            }
        }
    }
}
