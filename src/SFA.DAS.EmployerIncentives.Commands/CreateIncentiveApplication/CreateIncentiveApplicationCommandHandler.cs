using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.Extensions;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
using SFA.DAS.EmployerIncentives.Domain.Factories;
using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Commands.Services;

namespace SFA.DAS.EmployerIncentives.Commands.CreateIncentiveApplication
{
    public class CreateIncentiveApplicationCommandHandler : ICommandHandler<CreateIncentiveApplicationCommand>
    {
        private readonly IIncentiveApplicationFactory _domainFactory;
        private readonly IIncentiveApplicationDomainRepository _domainRepository;
        private readonly IIncentivePaymentProfilesService _incentivePaymentProfilesService;

        public CreateIncentiveApplicationCommandHandler(IIncentiveApplicationFactory domainFactory, IIncentiveApplicationDomainRepository domainRepository, IIncentivePaymentProfilesService incentivePaymentProfilesService)
        {
            _domainFactory = domainFactory;
            _domainRepository = domainRepository;
            _incentivePaymentProfilesService = incentivePaymentProfilesService;
        }

        public async Task Handle(CreateIncentiveApplicationCommand command, CancellationToken cancellationToken = default)
        {
            var application = _domainFactory.CreateNew(command.IncentiveApplicationId, command.AccountId, command.AccountLegalEntityId);
            application.SetApprenticeships(command.Apprenticeships.ToEntities(_domainFactory, _incentivePaymentProfilesService));

            await _domainRepository.Save(application);
        }

    }
}
