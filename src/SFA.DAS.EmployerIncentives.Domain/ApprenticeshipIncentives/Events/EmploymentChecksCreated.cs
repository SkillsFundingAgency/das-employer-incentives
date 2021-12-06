using SFA.DAS.EmployerIncentives.Abstractions.Logging;
using SFA.DAS.EmployerIncentives.Domain.IncentiveApplications;
using SFA.DAS.EmployerIncentives.Domain.ValueObjects;
using System;

namespace SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Events
{
    public class EmploymentChecksCreated : IDomainEvent, ILogWriter
    {
        public Guid ApprenticeshipIncentiveId { get; set; }
        public ServiceRequest ServiceRequest { get; set; }

        public EmploymentChecksCreated(Guid apprenticeshipIncentiveId) : this(apprenticeshipIncentiveId, null)
        {
        }

        public EmploymentChecksCreated(Guid apprenticeshipIncentiveId, ServiceRequest serviceRequest)
        {
            ApprenticeshipIncentiveId = apprenticeshipIncentiveId;
            ServiceRequest = serviceRequest;
        }

        public Log Log
        {
            get
            {

                var additionalMessage = ServiceRequest == null ? string.Empty : $", ServiceReqest {ServiceRequest.TaskId}";
                var message = $"Employment Checks Created for Apprenticeship Incentive with ApprenticeshipIncentiveId {ApprenticeshipIncentiveId} {additionalMessage}";
                return new Log
                {
                    OnProcessing = () => message,
                    OnError = () => message
                };
            }
        }
    }
}
