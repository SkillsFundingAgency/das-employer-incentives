using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Domain.Factories;

namespace SFA.DAS.EmployerIncentives.Commands.CreateIncentiveApplication
{
    public class CreateIncentiveApplicationCommandHandler : ICommandHandler<CreateIncentiveApplicationCommand>
    {
        private readonly IIncentiveApplicationFactory _domainFactory;
        private readonly IIncentiveApplicationDomainRepository _domainRepository;

        public CreateIncentiveApplicationCommandHandler(IIncentiveApplicationFactory domainFactory, IIncentiveApplicationDomainRepository domainRepository)
        {
            _domainFactory = domainFactory;
            _domainRepository = domainRepository;
        }

        public async Task Handle(CreateIncentiveApplicationCommand command, CancellationToken cancellationToken = default)
        {
            var incentiveApplication = _domainFactory.CreateNew(command.IncentiveApplicationId, command.AccountId, command.AccountLegalEntityId);
            foreach (var apprenticeship in command.Apprenticeships)
            {
                incentiveApplication.AddApprenticeship(_domainFactory.CreateNewApprenticeship(apprenticeship.Id, apprenticeship.ApprenticeshipId, apprenticeship.FirstName, apprenticeship.LastName, apprenticeship.DateOfBirth, apprenticeship.Uln, apprenticeship.PlannedStartDate, apprenticeship.ApprenticeshipEmployerTypeOnApproval));
            }

            await _domainRepository.Save(incentiveApplication);
        }
    }
}
