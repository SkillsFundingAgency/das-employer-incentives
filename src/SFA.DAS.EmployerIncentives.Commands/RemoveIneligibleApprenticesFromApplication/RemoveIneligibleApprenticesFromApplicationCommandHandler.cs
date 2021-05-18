using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.Exceptions;
using SFA.DAS.EmployerIncentives.Commands.Persistence;

namespace SFA.DAS.EmployerIncentives.Commands.RemoveIneligibleApprenticesFromApplication
{
    public class RemoveIneligibleApprenticesFromApplicationCommandHandler : ICommandHandler<RemoveIneligibleApprenticesFromApplicationCommand>
    {
        private readonly IIncentiveApplicationDomainRepository _domainRepository;        

        public RemoveIneligibleApprenticesFromApplicationCommandHandler(IIncentiveApplicationDomainRepository domainRepository)
        {
            _domainRepository = domainRepository;
        }

        public async Task Handle(RemoveIneligibleApprenticesFromApplicationCommand command, CancellationToken cancellationToken = default)
        {
            var application = await _domainRepository.Find(command.IncentiveApplicationId);

            if (application == null || application.AccountId != command.AccountId)
            {
                throw new InvalidRequestException();
            }
            
            application.RemoveApprenticeshipsWithIneligibleStartDates();

            await _domainRepository.Save(application);
        }
    }
}
