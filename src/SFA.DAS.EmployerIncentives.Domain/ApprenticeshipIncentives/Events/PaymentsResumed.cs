using SFA.DAS.EmployerIncentives.Abstractions.Logging;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Domain.IncentiveApplications;
using SFA.DAS.EmployerIncentives.Domain.ValueObjects;

namespace SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Events
{
    public class PaymentsResumed : IDomainEvent, ILogWriter
    {
        public long AccountId { get; }
        public long AccountLegalEntityId { get; }
        public ApprenticeshipIncentiveModel Model { get; }
        public ServiceRequest ServiceRequest { get; }

        public PaymentsResumed(
            long accountId,
            long accountLegalEntityId,
            ApprenticeshipIncentiveModel model,
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
                    $"Paused Apprenticeship Incentive has been resumed for ApprenticeshipIncentiveId {Model.ApplicationApprenticeshipId} and " +
                    $"AccountLegalEntityId {AccountLegalEntityId} and ULN {Model.Apprenticeship.UniqueLearnerNumber}" + 
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
