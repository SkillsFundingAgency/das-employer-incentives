using System.Linq;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.Extensions;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
using SFA.DAS.EmployerIncentives.Domain.Factories;
using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Commands.Services;

namespace SFA.DAS.EmployerIncentives.Commands.UpdateIncentiveApplication
{
    public class UpdateIncentiveApplicationCommandHandler : ICommandHandler<UpdateIncentiveApplicationCommand>
    {
        private readonly IIncentiveApplicationFactory _domainFactory;
        private readonly IIncentiveApplicationDomainRepository _domainRepository;
        private readonly IIncentivePaymentProfilesService _incentivePaymentProfilesService;

        public UpdateIncentiveApplicationCommandHandler(IIncentiveApplicationFactory domainFactory, IIncentiveApplicationDomainRepository domainRepository, IIncentivePaymentProfilesService incentivePaymentProfilesService)
        {
            _domainFactory = domainFactory;
            _domainRepository = domainRepository;
            _incentivePaymentProfilesService = incentivePaymentProfilesService;
        }

        public async Task Handle(UpdateIncentiveApplicationCommand command, CancellationToken cancellationToken = default)
        {
            var application = await _domainRepository.Find(command.IncentiveApplicationId);
            application.SetApprenticeships(command.Apprenticeships.ToEntities(_domainFactory, _incentivePaymentProfilesService));

            await _domainRepository.Save(application);
        }
    }
}
