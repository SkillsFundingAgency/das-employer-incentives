using System;
using System.Collections.Generic;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Abstractions.DTOs;

namespace SFA.DAS.EmployerIncentives.Commands.CreateIncentiveApplication
{    
    public class CreateIncentiveApplicationCommand : ICommand
    {
        public Guid IncentiveApplicationId { get; private set; }
        public long AccountId { get; private set; }
        public long AccountLegalEntityId { get; private set; }
        public IEnumerable<IncentiveClaimApprenticeshipDto> Apprenticeships { get; private set; }

        public CreateIncentiveApplicationCommand(
            Guid incentiveApplicationId,
            long accountId,
            long accountLegalEntityId,
            IEnumerable<IncentiveClaimApprenticeshipDto> apprenticeships)
        {
            IncentiveApplicationId = incentiveApplicationId;
            AccountId = accountId;
            AccountLegalEntityId = accountLegalEntityId;
            Apprenticeships = apprenticeships;
        }
    }
}
