using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Abstractions.Events;
using SFA.DAS.EmployerIncentives.Data.EarningsResilienceCheck;
using SFA.DAS.EmployerIncentives.Domain.EarningsResilienceCheck.Events;
using SFA.DAS.EmployerIncentives.Domain.IncentiveApplications.Models;
using SFA.DAS.EmployerIncentives.Queries.EarningsResilienceCheck;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Commands.EarningsResilienceCheck
{
    public class EarningsResilienceCheckHandler : ICommandHandler<EarningsResilienceCheckCommand>
    {
        private readonly IEarningsResilienceCheckRepository _resilienceCheckRepository;
        private readonly IDomainEventDispatcher _domainEventDispatcher;

        public EarningsResilienceCheckHandler(IEarningsResilienceCheckRepository resilienceCheckRepository, IDomainEventDispatcher domainEventDispatcher)
        {
            _resilienceCheckRepository = resilienceCheckRepository;
            _domainEventDispatcher = domainEventDispatcher;
        }

        public async Task Handle(EarningsResilienceCheckCommand command, CancellationToken cancellationToken = default)
        {
            var applicationIds = await _resilienceCheckRepository.GetApplicationsWithoutEarningsCalculations();

            foreach (var applicationId in applicationIds)
            {
                var applicationDetail = await _resilienceCheckRepository.GetApplicationDetail(applicationId);
                if (applicationDetail != null)
                {
                    await GenerateEventForApprenticeshipsWithoutEarningsCalculations(applicationDetail);
                }
            }
        }

        private async Task GenerateEventForApprenticeshipsWithoutEarningsCalculations(IncentiveApplicationModel applicationDetail)
        {
            applicationDetail.ApprenticeshipModels = new Collection<ApprenticeshipModel>(applicationDetail.ApprenticeshipModels.Where(x => x.EarningsCalculated == false).ToList());
            if (applicationDetail.ApprenticeshipModels.Count() > 0)
            {
                var earningsCalculationEvent = new EarningsCalculationRequired(applicationDetail);
                await _domainEventDispatcher.Send(earningsCalculationEvent);
            }
        }
    }
}
