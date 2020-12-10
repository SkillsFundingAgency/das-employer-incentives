using SFA.DAS.EmployerIncentives.Abstractions.Logging;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;
using SFA.DAS.EmployerIncentives.Domain.IncentiveApplications;
using SFA.DAS.EmployerIncentives.Domain.ValueObjects;

namespace SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Events
{
    public class PaymentsPaused : IDomainEvent, ILogWriter
    {
        public long AccountId { get; }
        public long AccountLegalEntityId { get; }
        public ValueTypes.Apprenticeship Apprenticeship { get; }
        public ServiceRequest ServiceRequest { get; }

        public PaymentsPaused(
            long accountId,
            long accountLegalEntityId,
            ValueTypes.Apprenticeship model,
            ServiceRequest serviceRequest)
        {
            AccountLegalEntityId = accountLegalEntityId;
            AccountId = accountId;
            Apprenticeship = model;
            ServiceRequest = serviceRequest;
        }

        public Log Log
        {
            get
            {
                var message =
                    $"Apprenticeship Incentive has been paused for ApprenticeshipIncentiveId {Apprenticeship.Id} and " +
                    $"AccountLegalEntityId {AccountLegalEntityId} and ULN {Apprenticeship.UniqueLearnerNumber}" + 
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
