using SFA.DAS.EmployerIncentives.Abstractions.Logging;
using SFA.DAS.EmployerIncentives.Domain.IncentiveApplications.Models;

namespace SFA.DAS.EmployerIncentives.Domain.IncentiveApplications.Events
{
    public class ApplicationReinstated : IDomainEvent, ILogWriter
    {
        public long AccountId { get; }
        public long AccountLegalEntityId { get; }
        public ApprenticeshipModel Model { get; }
        
        public ApplicationReinstated(
            long accountId,
            long accountLegalEntityId, 
            ApprenticeshipModel model)
        {
            AccountLegalEntityId = accountLegalEntityId;
            AccountId = accountId;
            Model = model;
        }

        public Log Log
        {
            get
            {
                var message =
                    $"Application Apprenticeship has been reinstated for ApprenticeshipId {Model.ApprenticeshipId} and " +
                    $"AccountLegalEntityId {AccountLegalEntityId} and ULN {Model.ULN}";
                return new Log
                {
                    OnProcessing = () => message,
                    OnError = () => message
                };
            }
        }
    }
}
