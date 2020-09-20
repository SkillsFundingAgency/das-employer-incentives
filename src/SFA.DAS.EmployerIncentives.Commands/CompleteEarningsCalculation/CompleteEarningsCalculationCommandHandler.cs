using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Abstractions.DTOs.Queries;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
using SFA.DAS.EmployerIncentives.Commands.Types.IncentiveApplications;
using SFA.DAS.EmployerIncentives.Data;

namespace SFA.DAS.EmployerIncentives.Commands.CompleteEarningsCalculation
{
    public class CompleteEarningsCalculationCommandHandler : ICommandHandler<CompleteEarningsCalculationCommand>
    {
        private readonly IIncentiveApplicationDomainRepository _domainRepository;
        private readonly IQueryRepository<IncentiveApplicationDto> _queryRepository;

        public CompleteEarningsCalculationCommandHandler(
            IQueryRepository<IncentiveApplicationDto> queryRepository,
            IIncentiveApplicationDomainRepository domainRepository)
        {
            _queryRepository = queryRepository;
            _domainRepository = domainRepository;
        }

        public async Task Handle(CompleteEarningsCalculationCommand command, CancellationToken cancellationToken = default)
        {
            var applicationDto = await _queryRepository.Get(app => app.AccountId == command.AccountId);
            var applicationApprenticeshipDto = applicationDto.Apprenticeships.Single(a => a.Id == command.IncentiveApplicationApprenticeshipId);

            var application = await _domainRepository.Find(applicationDto.Id);

            application.EarningsCalculated(applicationApprenticeshipDto.Id);

            await _domainRepository.Save(application);
        }
    }
}
