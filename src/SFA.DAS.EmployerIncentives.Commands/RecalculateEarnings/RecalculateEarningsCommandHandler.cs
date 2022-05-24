using System;
using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
using SFA.DAS.EmployerIncentives.Commands.Types.ApprenticeshipIncentive;
using SFA.DAS.EmployerIncentives.Domain.Interfaces;

namespace SFA.DAS.EmployerIncentives.Commands.RecalculateEarnings
{
    public class RecalculateEarningsCommandHandler : ICommandHandler<RecalculateEarningsCommand>
    {
        private readonly IApprenticeshipIncentiveDomainRepository _domainRepository;
        private readonly ICollectionCalendarService _collectionCalendarService;

        public RecalculateEarningsCommandHandler(IApprenticeshipIncentiveDomainRepository domainRepository, ICollectionCalendarService collectionCalendarService)
        {
            _domainRepository = domainRepository;
            _collectionCalendarService = collectionCalendarService;
        }

        public async Task Handle(RecalculateEarningsCommand command, CancellationToken cancellationToken = default(CancellationToken))
        {
            var collectionCalendar = await _collectionCalendarService.Get();
            
            foreach(var learnerIdentifier in command.IncentiveLearnerIdentifiers)
            {
                var incentive = await _domainRepository.FindByUlnWithinAccountLegalEntity(learnerIdentifier.ULN, learnerIdentifier.AccountLegalEntityId);

                if (incentive == null)
                {
                    throw new ArgumentException($"Apprenticeship incentive not found for account legal entity {learnerIdentifier.AccountLegalEntityId} and ULN {learnerIdentifier.ULN}");
                }

                incentive.RecalculateEarnings(collectionCalendar);
                await _domainRepository.Save(incentive);
            }
        }
    }
}
