using SFA.DAS.EmployerIncentives.Abstractions.Logging;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Domain.IncentiveApplications;

namespace SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Events
{
    public class EmploymentCheckDeleted : IDomainEvent, ILogWriter
    {
        public EmploymentCheckModel Model { get; }

        public EmploymentCheckDeleted(EmploymentCheckModel model)
        {
            Model = model;
        }

        public Log Log
        {
            get
            {
                var message = $"EmploymentCheck has been deleted for  Apprenticeship Incentive with ApprenticeshipIncentiveId {Model.ApprenticeshipIncentiveId}";
                return new Log
                {
                    OnProcessing = () => message,
                    OnError = () => message
                };
            }
        }
    }
}
