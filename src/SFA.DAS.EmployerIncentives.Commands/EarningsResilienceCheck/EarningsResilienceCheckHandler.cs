using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.Exceptions;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
using SFA.DAS.EmployerIncentives.Data.EarningsResilienceCheck;
using SFA.DAS.EmployerIncentives.Queries.EarningsResilienceCheck;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Commands.EarningsResilienceCheck
{
    public class EarningsResilienceCheckHandler : ICommandHandler<EarningsResilienceCheckCommand>
    {
        private readonly IEarningsResilienceCheckRepository _resilienceCheckRepository;
        private readonly IIncentiveApplicationDomainRepository _domainRepository;

        public EarningsResilienceCheckHandler(IEarningsResilienceCheckRepository resilienceCheckRepository, IIncentiveApplicationDomainRepository domainRepository)
        {
            _resilienceCheckRepository = resilienceCheckRepository;
            _domainRepository = domainRepository;
        }

        public async Task Handle(EarningsResilienceCheckCommand command, CancellationToken cancellationToken = default)
        {
            var applicationIds = await _resilienceCheckRepository.GetApplicationsWithoutEarningsCalculations();

            foreach (var applicationId in applicationIds)
            {
                var application = await _domainRepository.Find(applicationId);

                if (application == null)
                {
                    throw new InvalidRequestException();
                }

                application.CalculateEarnings();

                await _domainRepository.Save(application);
            }
        }

    }
}
