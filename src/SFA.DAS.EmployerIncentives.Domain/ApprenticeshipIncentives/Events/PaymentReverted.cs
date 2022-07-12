using SFA.DAS.EmployerIncentives.Abstractions.Logging;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Domain.IncentiveApplications;
using SFA.DAS.EmployerIncentives.Domain.ValueObjects;

namespace SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Events
{
    public class PaymentReverted : IDomainEvent, ILogWriter
    {
        public PaymentModel Model { get; }
        public ServiceRequest ServiceRequest { get; }

        public PaymentReverted(PaymentModel model, ServiceRequest serviceRequest)
        {
            Model = model;
            ServiceRequest = serviceRequest;
        }

        public Log Log
        {
            get
            {
                var message =
                    $"Payment has been reverted for Apprenticeship Incentive with ApprenticeshipIncentiveId {Model.ApprenticeshipIncentiveId} and " +
                    $"PendingPaymentId {Model.PendingPaymentId} for Service Request Id {ServiceRequest.TaskId}";
                return new Log
                {
                    OnProcessing = () => message,
                    OnError = () => message
                };
            }
        }
    }
}
