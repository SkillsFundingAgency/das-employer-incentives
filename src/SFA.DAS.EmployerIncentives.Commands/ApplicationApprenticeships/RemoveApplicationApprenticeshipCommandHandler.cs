using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.Exceptions;
using SFA.DAS.EmployerIncentives.Commands.Persistence;

namespace SFA.DAS.EmployerIncentives.Commands.ApplicationApprenticeships
{
    public class RemoveApplicationApprenticeshipCommandHandler : ICommandHandler<RemoveApplicationApprenticeshipCommand>
    {
        private readonly IIncentiveApplicationDomainRepository _domainRepository;

        public RemoveApplicationApprenticeshipCommandHandler(IIncentiveApplicationDomainRepository domainRepository)
        {
            _domainRepository = domainRepository;
        }

        public async Task Handle(RemoveApplicationApprenticeshipCommand command, CancellationToken cancellationToken = default(CancellationToken))
        {
            var application = await _domainRepository.Find(command.IncentiveApplicationId);
            
            if (application == null)
            {
                throw new InvalidRequestException();
            }

            application.RemoveApprenticeship(command.ApprenticeshipId);

            await _domainRepository.Save(application);
        }
    }
}
