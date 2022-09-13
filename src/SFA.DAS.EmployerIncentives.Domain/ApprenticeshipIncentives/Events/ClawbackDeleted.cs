using SFA.DAS.EmployerIncentives.Abstractions.Logging;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Domain.IncentiveApplications;

namespace SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Events
{
    public class ClawbackDeleted : IDomainEvent, ILogWriter
    {
        public long AccountId { get; }
        public long AccountLegalEntityId { get; }
        public long UniqueLearnerNumber { get; }
        public ClawbackPaymentModel Model { get; }

        public ClawbackDeleted(
            long accountId,
            long accountLegalEntityId,
            long uniqueLearnerNumber,
            ClawbackPaymentModel model)
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
                    $"ClawbackPayment has been deleted for  Apprenticeship Incentive with ApprenticeshipIncentiveId {Model.ApprenticeshipIncentiveId} and " +
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
