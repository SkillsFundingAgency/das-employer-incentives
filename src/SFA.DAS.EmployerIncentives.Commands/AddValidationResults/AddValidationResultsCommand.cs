using System;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;

namespace SFA.DAS.EmployerIncentives.Commands.AddValidationResults
{
    public class AddValidationResultsCommand : ICommand
    {
        public  Guid PendingPaymentId { get; }
        public bool HasVendorAssigned { get; }
        public Guid ApprenticeshipIncentiveId { get; }

        public AddValidationResultsCommand(Guid apprenticeshipIncentiveId, Guid pendingPaymentId, bool hasVendorAssigned)
        {
            PendingPaymentId = pendingPaymentId;
            HasVendorAssigned = hasVendorAssigned;
            ApprenticeshipIncentiveId = apprenticeshipIncentiveId;
        }
    }
}
