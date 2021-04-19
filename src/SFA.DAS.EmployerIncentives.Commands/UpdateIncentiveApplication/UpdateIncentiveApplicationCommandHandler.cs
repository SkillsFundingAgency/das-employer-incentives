using System.Linq;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.Extensions;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
using SFA.DAS.EmployerIncentives.Domain.Factories;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Commands.UpdateIncentiveApplication
{
    public class UpdateIncentiveApplicationCommandHandler : ICommandHandler<UpdateIncentiveApplicationCommand>
    {
        private readonly IIncentiveApplicationFactory _domainFactory;
        private readonly IIncentiveApplicationDomainRepository _domainRepository;

        public UpdateIncentiveApplicationCommandHandler(IIncentiveApplicationFactory domainFactory, IIncentiveApplicationDomainRepository domainRepository)
        {
            _domainFactory = domainFactory;
            _domainRepository = domainRepository;
        }

        public async Task Handle(UpdateIncentiveApplicationCommand command, CancellationToken cancellationToken = default)
        {
            var application = await _domainRepository.Find(command.IncentiveApplicationId);
            application.AddApprenticeships(command.Apprenticeships.Where(x => x.Selected).ToEntities(_domainFactory));
            application.RemoveApprenticeships(command.Apprenticeships.Where(x => !x.Selected).ToEntities(_domainFactory));
            await _domainRepository.Save(application);
        }
    }
}
