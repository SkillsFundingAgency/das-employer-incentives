using System;
using SFA.DAS.EmployerIncentives.Abstractions.Logging;
using SFA.DAS.EmployerIncentives.Domain.IncentiveApplications;
using SFA.DAS.EmployerIncentives.Domain.ValueObjects;

namespace SFA.DAS.EmployerIncentives.Domain.Accounts.Events
{
    public class VendorBlockCreated : IDomainEvent, ILogWriter
    {
        public string VendorId { get; set; }
        public DateTime VendorBlockEndDate { get; set; }
        public ServiceRequest ServiceRequest { get; set; }

        public VendorBlockCreated(string vendorId, DateTime vendorBlockEndDate) : this(vendorId, vendorBlockEndDate, null)
        {
        }

        public VendorBlockCreated(string vendorId, DateTime vendorBlockEndDate, ServiceRequest serviceRequest)
        {
            VendorId = vendorId;
            VendorBlockEndDate = vendorBlockEndDate;
            ServiceRequest = serviceRequest;
        }

        public Log Log
        {
            get
            {
                var additionalMessage = ServiceRequest == null ? string.Empty : $", ServiceRequest {ServiceRequest.TaskId}";
                var message = $"Vendor Block Created for Accounts with VendorId {VendorId} End Date {VendorBlockEndDate} {additionalMessage}";
                return new Log
                {
                    OnProcessing = () => message,
                    OnError = () => message
                };
            }
        }
    }
}