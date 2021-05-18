using System;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;

namespace SFA.DAS.EmployerIncentives.Commands.RemoveIneligibleApprenticesFromApplication
{
    public class RemoveIneligibleApprenticesFromApplicationCommand : ICommand
    {
        public Guid IncentiveApplicationId { get; }
        public long AccountId { get; }

        public RemoveIneligibleApprenticesFromApplicationCommand(
            Guid incentiveApplicationId,
            long accountId)
        {
            IncentiveApplicationId = incentiveApplicationId;
            AccountId = accountId;
        }
    }
}
