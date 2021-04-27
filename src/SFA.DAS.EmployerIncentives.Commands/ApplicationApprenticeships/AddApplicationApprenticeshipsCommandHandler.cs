using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.Exceptions;
using SFA.DAS.EmployerIncentives.Commands.Extensions;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
using SFA.DAS.EmployerIncentives.Domain.Factories;

namespace SFA.DAS.EmployerIncentives.Commands.ApplicationApprenticeships
{
    public class AddApplicationApprenticeshipsCommandHandler : ICommandHandler<AddApplicationApprenticeshipsCommand>
    {
        private readonly IIncentiveApplicationDomainRepository _domainRepository;
        private readonly IIncentiveApplicationFactory _domainFactory;

        public AddApplicationApprenticeshipsCommandHandler(IIncentiveApplicationDomainRepository domainRepository, IIncentiveApplicationFactory domainFactory)
        {
            _domainRepository = domainRepository;
            _domainFactory = domainFactory;
        }

        public async Task Handle(AddApplicationApprenticeshipsCommand command, CancellationToken cancellationToken = default(CancellationToken))
        {
            var application = await _domainRepository.Find(command.IncentiveApplicationId);

            if (application == null || application.AccountId != command.AccountId)
            {
                throw new InvalidRequestException();
            }

            application.AddApprenticeships(command.Apprenticeships.ToEntities(_domainFactory));

            await _domainRepository.Save(application);
        }
    }
}
