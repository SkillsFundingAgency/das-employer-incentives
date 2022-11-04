using SFA.DAS.EmployerIncentives.Abstractions.Logging;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Domain.IncentiveApplications;
using SFA.DAS.EmployerIncentives.Domain.ValueObjects;
using System;

namespace SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Events
{
    public class EmploymentChecksCreated : IDomainEvent, ILogWriter
    {
        public Guid ApprenticeshipIncentiveId { get; }
        public EmploymentCheckModel Model { get; }
        public ServiceRequest ServiceRequest { get; set; }

        public EmploymentChecksCreated(EmploymentCheckModel model) : this(model, null)
        {
        }

        public EmploymentChecksCreated(EmploymentCheckModel model, ServiceRequest serviceRequest)
        {
            ApprenticeshipIncentiveId = model.ApprenticeshipIncentiveId;
            Model = model;
            ServiceRequest = serviceRequest;
        }

        public Log Log
        {
            get
            {

                var additionalMessage = ServiceRequest == null ? string.Empty : $", ServiceReqest {ServiceRequest.TaskId}";
                var message = $"Employment Check {Model.CheckType} Created for Apprenticeship Incentive with ApprenticeshipIncentiveId {ApprenticeshipIncentiveId} {additionalMessage}";
                return new Log
                {
                    OnProcessing = () => message,
                    OnError = () => message
                };
            }
        }
    }
}
