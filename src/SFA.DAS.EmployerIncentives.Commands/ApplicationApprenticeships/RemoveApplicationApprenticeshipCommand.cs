using System;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;

namespace SFA.DAS.EmployerIncentives.Commands.ApplicationApprenticeships
{
    public class RemoveApplicationApprenticeshipCommand : ICommand
    {
        public Guid IncentiveApplicationId { get; }
        public long ApprenticeshipId { get; }

        public RemoveApplicationApprenticeshipCommand(
            Guid incentiveApplicationId,
            long apprenticeshipId)
        {
            IncentiveApplicationId = incentiveApplicationId;
            ApprenticeshipId = apprenticeshipId;
        }
    }
}
