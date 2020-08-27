using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Abstractions.DTOs.Queries;
using SFA.DAS.EmployerIncentives.Data;
using SFA.DAS.EmployerIncentives.Enums;
using SFA.DAS.EmployerIncentives.Messages.Events;
using SFA.DAS.NServiceBus.Services;

namespace SFA.DAS.EmployerIncentives.Commands.GetVrfCaseDetailsForNewSubmissions
{
    public class GetVrfCaseDetailsForNewSubmissionsCommandHandler : ICommandHandler<GetVrfCaseDetailsForNewSubmissionsCommand>
    {
        private readonly IQueryRepository<IncentiveApplicationLegalEntityDto> _queryRepository;
        private readonly IEventPublisher _eventPublisher;

        public GetVrfCaseDetailsForNewSubmissionsCommandHandler(IQueryRepository<IncentiveApplicationLegalEntityDto> queryRepository, IEventPublisher eventPublisher)
        {
            _queryRepository = queryRepository;
            _eventPublisher = eventPublisher;
        }
        public async Task Handle(GetVrfCaseDetailsForNewSubmissionsCommand command, CancellationToken cancellationToken = default)
        {
            var applicationLegalEntities = await _queryRepository.GetList(x => x.ApplicationStatus == IncentiveApplicationStatus.Submitted && x.VrfCaseId == null);
            var publishTasks = applicationLegalEntities.Select(x => _eventPublisher.Publish(new GetLegalEntityVrfCaseDetailsEvent { LegalEntityId = x.LegalEntityId }));
            await Task.WhenAll(publishTasks);
        }
    }
}

