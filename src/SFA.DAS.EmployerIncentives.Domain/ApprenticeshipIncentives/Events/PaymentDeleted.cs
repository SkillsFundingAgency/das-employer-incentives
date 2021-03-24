using SFA.DAS.EmployerIncentives.Abstractions.Logging;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Domain.IncentiveApplications;

namespace SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Events
{
    public class PaymentDeleted : IDomainEvent, ILogWriter
    {
        public long AccountId { get; }
        public long AccountLegalEntityId { get; }
        public long UniqueLearnerNumber { get; }
        public PaymentModel Model { get; }

        public PaymentDeleted(
            long accountId,
            long accountLegalEntityId,
            long uniqueLearnerNumber,
            PaymentModel model)
        {
            AccountLegalEntityId = accountLegalEntityId;
            AccountId = accountId;
            UniqueLearnerNumber = uniqueLearnerNumber;
            Model = model;
        }

        public Log Log
        {
            get
            {
                var message =
                    $"Payment has been deleted for  Apprenticeship Incentive with ApprenticeshipIncentiveId {Model.ApprenticeshipIncentiveId} and " +
                    $"AccountLegalEntityId {AccountLegalEntityId} and ULN {UniqueLearnerNumber}" ;
                return new Log
                {
                    OnProcessing = () => message,
                    OnError = () => message
                };
            }
        }
    }
}
