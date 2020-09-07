using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.Exceptions;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
using SFA.DAS.EmployerIncentives.Commands.Types.Application;
using SFA.DAS.EmployerIncentives.Infrastructure.Configuration;

namespace SFA.DAS.EmployerIncentives.Commands.IncentiveApplicationCalculateClaim
{
    public class CalculateClaimCommandHandler : ICommandHandler<CalculateClaimCommand>
    {
        private readonly IIncentiveApplicationDomainRepository _domainRepository;
        private readonly ApplicationSettings _applicationSettings;

        public CalculateClaimCommandHandler(IIncentiveApplicationDomainRepository domainRepository, IOptions<ApplicationSettings> applicationSettings)
        {
            _domainRepository = domainRepository;
            _applicationSettings = applicationSettings.Value;
        }

        public async Task Handle(CalculateClaimCommand command, CancellationToken cancellationToken = default)
        {
            var application = await _domainRepository.Find(command.IncentiveClaimApplicationId);

            if (application == null || application.AccountId != command.AccountId)
            {
                throw new InvalidRequestException();
            }

            application.CalculateClaim(_applicationSettings.IncentivePaymentProfiles);            

            await _domainRepository.Save(application);
        }
    }
}
