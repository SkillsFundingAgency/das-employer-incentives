using System;
using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
using SFA.DAS.EmployerIncentives.Commands.Types.IncentiveApplications;
using SFA.DAS.EmployerIncentives.Data;
using SFA.DAS.EmployerIncentives.Data.Models;

namespace SFA.DAS.EmployerIncentives.Commands.CompleteEarningsCalculation
{
    public class CompleteEarningsCalculationCommandHandler : ICommandHandler<CompleteEarningsCalculationCommand>
    {
        private readonly IIncentiveApplicationDomainRepository _domainRepository;
        private readonly IQueryRepository<IncentiveApplicationApprenticeship> _apprenticeshipQueryRepository;

        public CompleteEarningsCalculationCommandHandler(
            IQueryRepository<IncentiveApplicationApprenticeship> apprenticeshipQueryRepository,
            IIncentiveApplicationDomainRepository domainRepository)
        {
            _apprenticeshipQueryRepository = apprenticeshipQueryRepository;
            _domainRepository = domainRepository;
        }

        public async Task Handle(CompleteEarningsCalculationCommand command, CancellationToken cancellationToken = default)
        {
            var apprenticeship = await _apprenticeshipQueryRepository.Get(app => app.Id == command.IncentiveApplicationApprenticeshipId);
            if (apprenticeship != null)
            {
                var application = await _domainRepository.Find(apprenticeship.IncentiveApplicationId);

                application.EarningsCalculated(apprenticeship.Id);

                await _domainRepository.Save(application);
            }
        }
    }
}
