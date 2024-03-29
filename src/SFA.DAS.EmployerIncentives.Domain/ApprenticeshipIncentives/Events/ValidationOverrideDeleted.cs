﻿using SFA.DAS.EmployerIncentives.Abstractions.Logging;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;
using SFA.DAS.EmployerIncentives.Domain.IncentiveApplications;
using SFA.DAS.EmployerIncentives.Domain.ValueObjects;
using System;
using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Events
{
    [ExcludeFromCodeCoverage]
    public class ValidationOverrideDeleted : IDomainEvent, ILogWriter
    {
        public Guid ValidationOverrideId { get; set; }
        public Guid ApprenticeshipIncentiveId { get; set; }
        public ValidationOverrideStep ValidationOverrideStep { get; set; }
        public ServiceRequest ServiceRequest { get; set; }

        public ValidationOverrideDeleted(
            Guid validationOverrideId,
            Guid apprenticeshipIncentiveId,
            ValidationOverrideStep validationOverrideStep,
            ServiceRequest serviceRequest
            )
        {
            ValidationOverrideId = validationOverrideId;
            ApprenticeshipIncentiveId = apprenticeshipIncentiveId;
            ValidationOverrideStep = validationOverrideStep;
            ServiceRequest = serviceRequest;
        }

        public Log Log
        {
            get
            {

                var additionalMessage = ServiceRequest == null ? string.Empty : $", ServiceReqest {ServiceRequest.TaskId}";
                var message = $"ValidationOverride '{ValidationOverrideStep}' Deleted for Apprenticeship Incentive with ApprenticeshipIncentiveId {ApprenticeshipIncentiveId} {additionalMessage}";
                return new Log
                {
                    OnProcessing = () => message,
                    OnError = () => message
                };
            }
        }
    }
}
