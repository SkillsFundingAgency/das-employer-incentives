using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.Extensions;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
using SFA.DAS.EmployerIncentives.Domain.Factories;
using System.Threading;
using System.Threading.Tasks;

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
            var application = _domainFactory.CreateNew(command.IncentiveApplicationId, command.AccountId, command.AccountLegalEntityId);
            application.SetApprenticeships(command.Apprenticeships.ToEntities(_domainFactory));

            await _domainRepository.Save(application);
        }

    }
}
