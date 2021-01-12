using SFA.DAS.EmployerIncentives.Abstractions.Logging;
using SFA.DAS.EmployerIncentives.Domain.IncentiveApplications.Models;
using SFA.DAS.EmployerIncentives.Domain.ValueObjects;
using SFA.DAS.EmployerIncentives.Enums;
using System;

namespace SFA.DAS.EmployerIncentives.Domain.IncentiveApplications.Events
{
    public abstract class ApplicationWithdrawn : IDomainEvent, ILogWriter
    {
        public IncentiveApplicationStatus WithdrawalStatus { get; }
        public long AccountId { get; }
        public long AccountLegalEntityId { get; }
        public ApprenticeshipModel Model { get; }
        public ServiceRequest ServiceRequest { get; }

        protected ApplicationWithdrawn(
            IncentiveApplicationStatus withdrawalStatus,
            long accountId,
            long accountLegalEntityId, 
            ApprenticeshipModel model,
            ServiceRequest serviceRequest)
        {
            WithdrawalStatus = withdrawalStatus;
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
                    $"Application Apprenticeship has been {Enum.GetName(typeof(IncentiveApplicationStatus), WithdrawalStatus)} for ApprenticeshipId {Model.ApprenticeshipId} and " +
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
