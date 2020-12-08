using SFA.DAS.EmployerIncentives.Abstractions.Logging;
using SFA.DAS.EmployerIncentives.Domain.IncentiveApplications.Models;
using SFA.DAS.EmployerIncentives.Domain.ValueObjects;

namespace SFA.DAS.EmployerIncentives.Domain.IncentiveApplications.Events
{
    public class EmployerWithdrawn : IDomainEvent, ILogWriter
    {
        public long AccountId { get; }
        public long AccountLegalEntityId { get; }
        public ApprenticeshipModel Model { get; }
        public ServiceRequest ServiceRequest { get; }

        public EmployerWithdrawn(
            long accountId,
            long accountLegalEntityId, 
            ApprenticeshipModel model,
            ServiceRequest serviceRequest)
        {
            AccountLegalEntityId = accountLegalEntityId;
            AccountId = accountId;
            Model = model;
            ServiceRequest = serviceRequest;
        }

        public Log Log
        {
            get
            {
                var message =
                    $"Application Apprenticeship has been withdrawn for ApprenticeshipId {Model.ApprenticeshipId} and " +
                    $"AccountLegalEntityId {AccountLegalEntityId} and ULN {Model.ULN}" + 
                    $"ServiceRequest TaskId {ServiceRequest.TaskId}";
                return new Log
                {
                    OnProcessing = () => message,
                    OnError = () => message
                };
            }
        }
    }
}
