using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
using SFA.DAS.EmployerIncentives.Commands.Types.ApprenticeshipIncentive;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;
using SFA.DAS.EmployerIncentives.Domain.Factories;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.CreateIncentive
{
    public class CreateIncentiveCommandHandler : ICommandHandler<CreateIncentiveCommand>
    {
        private readonly IIncentiveApplicationDomainRepository _applicationDomainRepository;
        private readonly IApprenticeshipIncentiveFactory _apprenticeshipIncentiveFactory;
        private readonly IApprenticeshipIncentiveDomainRepository _apprenticeshipIncentiveDomainRepository;
        

        public CreateIncentiveCommandHandler(
            IIncentiveApplicationDomainRepository applicationDomainRepository,
            IApprenticeshipIncentiveFactory apprenticeshipIncentiveFactory,
            IApprenticeshipIncentiveDomainRepository apprenticeshipIncentiveDomainRepository)
        {
            _applicationDomainRepository = applicationDomainRepository;
            _apprenticeshipIncentiveFactory = apprenticeshipIncentiveFactory;
            _apprenticeshipIncentiveDomainRepository = apprenticeshipIncentiveDomainRepository;
        }

        public async Task Handle(CreateIncentiveCommand command, CancellationToken cancellationToken = default)
        {
            var application = await _applicationDomainRepository.Find(command.IncentiveApplicationId);

            foreach (var apprenticeship in application.Apprenticeships)
            {
                var incentive = _apprenticeshipIncentiveFactory.CreateNew(
                    Guid.NewGuid(),
                    apprenticeship.Id,
                    new Account(application.AccountId, command.AccountLegalEntityId),
                    new Apprenticeship(
                        apprenticeship.ApprenticeshipId,
                        apprenticeship.FirstName,
                        apprenticeship.LastName,
                        apprenticeship.DateOfBirth,
                        apprenticeship.ULN,
                        apprenticeship.ApprenticeshipEmployerTypeOnApproval
                    ),
                    apprenticeship.PlannedStartDate);

                if (apprenticeship.UKPRN.HasValue)
                {
                    incentive.Apprenticeship.SetProvider(new Provider(apprenticeship.UKPRN.Value));
                }

                await _apprenticeshipIncentiveDomainRepository.Save(incentive);
            }
        }
    }
}
