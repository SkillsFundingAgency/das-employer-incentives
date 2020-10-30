using SFA.DAS.EmployerIncentives.Abstractions.Logging;
using SFA.DAS.EmployerIncentives.Domain.IncentiveApplications.Models;

namespace SFA.DAS.EmployerIncentives.Domain.IncentiveApplications.Events
{
    public class Submitted : IDomainEvent, ILogWriter
    {
        public IncentiveApplicationModel Model { get; }

        public Submitted(IncentiveApplicationModel model)
        {
            Model = model;
        }

        public Log Log
        {
            get
            {
                var message =
                    $"Application Submitted event with AccountId {Model.AccountId} and " +
                    $"AccountLegalEntityId {Model.AccountLegalEntityId} on {Model.DateSubmitted} by {Model.SubmittedByName}";
                return new Log
                {
                    OnProcessing = () => message,
                    OnError = () => message
                };
            }
        }
    }
}
