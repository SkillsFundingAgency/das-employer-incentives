using SFA.DAS.EmployerIncentives.Abstractions.Logging;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Domain.IncentiveApplications;

namespace SFA.DAS.EmployerIncentives.Domain.EarningsResilienceCheck.Events
{
    public class PaymentsCalculationRequired : IDomainEvent, ILogWriter
    {
        public ApprenticeshipIncentiveModel Model { get; }

        public PaymentsCalculationRequired(ApprenticeshipIncentiveModel model)
        {
            Model = model;
        }

        public Log Log
        {
            get
            {
                var message = $"Payments Calculation Required event with ApprenticeshipId {Model.Apprenticeship.Id} and ApprenticeshipIncentiveId {Model.Id}";
                return new Log
                {
                    OnProcessing = () => message,
                    OnError = () => message
                };
            }
        }
    }
}
