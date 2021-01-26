using SFA.DAS.EmployerIncentives.Abstractions.Logging;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Domain.IncentiveApplications;

namespace SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Events
{
    public class ClawBackAdded : IDomainEvent, ILogWriter
    {
        public ApprenticeshipIncentiveModel Model { get; }

        public ClawBackAdded(ApprenticeshipIncentiveModel model)
        {
            Model = model;
        }

        public Log Log
        {
            get
            {
                var message =
                    $"Apprenticeship Incentive clawback has been added for ApprenticeshipIncentiveId {Model.ApplicationApprenticeshipId} and " +
                    $"AccountLegalEntityId {Model.Account.AccountLegalEntityId} and ULN {Model.Apprenticeship.UniqueLearnerNumber}";
                return new Log
                {
                    OnProcessing = () => message,
                    OnError = () => message
                };
            }
        }
    }
}
