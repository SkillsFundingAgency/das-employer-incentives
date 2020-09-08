using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.Exceptions;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
using SFA.DAS.EmployerIncentives.Commands.Services;
using SFA.DAS.EmployerIncentives.Commands.Types.Application;

namespace SFA.DAS.EmployerIncentives.Commands.IncentiveApplicationCalculateClaim
{
    public class CalculateClaimCommandHandler : ICommandHandler<CalculateClaimCommand>
    {
        private readonly IIncentiveApplicationDomainRepository _domainRepository;
        private readonly IIncentivePaymentProfilesService _incentivePaymentProfilesService;

        public CalculateClaimCommandHandler(IIncentiveApplicationDomainRepository domainRepository, IIncentivePaymentProfilesService incentivePaymentProfilesService)
        {
            _domainRepository = domainRepository;
            _incentivePaymentProfilesService = incentivePaymentProfilesService;
        }

        public async Task Handle(CalculateClaimCommand command, CancellationToken cancellationToken = default)
        {
            var application = await _domainRepository.Find(command.IncentiveClaimApplicationId);

            if (application == null || application.AccountId != command.AccountId)
            {
                throw new InvalidRequestException();
            }

            application.CalculateClaim(_incentivePaymentProfilesService.MapToDomainIncentivePaymentProfiles().ToList());            

            await _domainRepository.Save(application);
        }
    }
}
