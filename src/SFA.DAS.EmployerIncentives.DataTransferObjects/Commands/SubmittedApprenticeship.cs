using System;

namespace SFA.DAS.EmployerIncentives.DataTransferObjects.Commands
{
    public class SubmittedApprenticeship
    {
        public Guid IncentiveApplicationId { get; }
        public Guid IncentiveApplicationApprenticeshipId { get; }
        public DateTime? DateSubmitted { get; }

        public SubmittedApprenticeship(Guid incentiveApplicationId, Guid incentiveApplicationApprenticeshipId, DateTime? dateSubmitted)
        {
            IncentiveApplicationId = incentiveApplicationId;
            IncentiveApplicationApprenticeshipId = incentiveApplicationApprenticeshipId;
            DateSubmitted = dateSubmitted;
        }
    }
}
