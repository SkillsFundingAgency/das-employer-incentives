using System;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;

namespace SFA.DAS.EmployerIncentives.Commands.RemoveIneligibleApprenticesFromApplication
{
    public class RemoveIneligibleApprenticesFromApplicationCommand : ICommand
    {
        public Guid IncentiveApplicationId { get; private set; }
        public long AccountId { get; private set; }

        public RemoveIneligibleApprenticesFromApplicationCommand(
            Guid incentiveApplicationId,
            long accountId)
        {
            IncentiveApplicationId = incentiveApplicationId;
            AccountId = accountId;
        }
    }
}
