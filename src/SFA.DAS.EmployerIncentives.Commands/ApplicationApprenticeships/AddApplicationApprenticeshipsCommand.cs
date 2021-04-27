using System;
using System.Collections.Generic;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Abstractions.DTOs.Commands;

namespace SFA.DAS.EmployerIncentives.Commands.ApplicationApprenticeships
{
    public class AddApplicationApprenticeshipsCommand : ICommand
    {
        public Guid IncentiveApplicationId { get; }
        public long AccountId { get; }
        public IEnumerable<IncentiveApplicationApprenticeshipDto> Apprenticeships { get; }

        public AddApplicationApprenticeshipsCommand(
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
