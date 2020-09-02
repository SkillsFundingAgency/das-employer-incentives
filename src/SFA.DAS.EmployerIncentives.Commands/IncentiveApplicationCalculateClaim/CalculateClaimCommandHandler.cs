using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.Exceptions;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
using SFA.DAS.EmployerIncentives.Commands.Types.Application;

namespace SFA.DAS.EmployerIncentives.Commands.IncentiveApplicationCalculateClaim
{
    public class CalculateClaimCommandHandler : ICommandHandler<CalculateClaimCommand>
    {
        private readonly IIncentiveApplicationDomainRepository _domainRepository;        

        public CalculateClaimCommandHandler(IIncentiveApplicationDomainRepository domainRepository)
        {
            _domainRepository = domainRepository;
        }

        public async Task Handle(CalculateClaimCommand command, CancellationToken cancellationToken = default)
        {
            var application = await _domainRepository.Find(command.IncentiveClaimApplicationId);

            if (application == null || application.AccountId != command.AccountId)
            {
                throw new InvalidRequestException();
            }

            foreach (var apprenticeship in application.Apprenticeships)
            {
                apprenticeship.StartClaimCalculation();
            }

            await _domainRepository.Save(application);
        }
    }
}
