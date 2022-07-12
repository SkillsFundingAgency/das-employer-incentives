using System;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;

namespace SFA.DAS.EmployerIncentives.Commands.VendorBlock
{
    public class BlockAccountLegalEntityForPaymentsCommand : ICommand
    {
        public string VendorId { get; }
        public DateTime VendorBlockEndDate { get; }

        public string ServiceRequestTaskId { get; }
        public string ServiceRequestDecisionReference { get; }
        public DateTime? ServiceRequestCreatedDate { get; }

        public BlockAccountLegalEntityForPaymentsCommand(string vendorId, DateTime vendorBlockEndDate, string serviceRequestTaskId, string serviceRequestDecisionReference, DateTime? serviceRequestCreatedDate)
        {
            VendorId = vendorId;
            VendorBlockEndDate = vendorBlockEndDate;
            ServiceRequestTaskId = serviceRequestTaskId;
            ServiceRequestDecisionReference = serviceRequestDecisionReference;
            ServiceRequestCreatedDate = serviceRequestCreatedDate;
        }
    }
}
