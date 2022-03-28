using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using System;
using System.Collections.Generic;
using SFA.DAS.EmployerIncentives.DataTransferObjects.Commands;

namespace SFA.DAS.EmployerIncentives.Commands.UpdateIncentiveApplication
{
    public class UpdateIncentiveApplicationCommand : ICommand
    {
        public Guid IncentiveApplicationId { get; }
        public long AccountId { get; }
        public IEnumerable<IncentiveApplicationApprenticeshipDto> Apprenticeships { get; }

        public UpdateIncentiveApplicationCommand(
            Guid incentiveApplicationId,
            long accountId,
            IEnumerable<IncentiveApplicationApprenticeshipDto> apprenticeships)
        {
            IncentiveApplicationId = incentiveApplicationId;
            AccountId = accountId;
            Apprenticeships = apprenticeships;
        }
    }
}
