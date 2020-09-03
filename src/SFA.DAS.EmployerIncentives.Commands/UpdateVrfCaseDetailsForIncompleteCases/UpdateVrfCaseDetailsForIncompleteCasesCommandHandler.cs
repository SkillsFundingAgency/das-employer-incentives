using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Abstractions.DTOs.Queries;
using SFA.DAS.EmployerIncentives.Data;
using SFA.DAS.EmployerIncentives.Messages.Events;
using SFA.DAS.NServiceBus.Services;

namespace SFA.DAS.EmployerIncentives.Commands.UpdateVrfCaseDetailsForIncompleteCases
{
    public class UpdateVrfCaseDetailsForIncompleteCasesCommandHandler : ICommandHandler<UpdateVrfCaseDetailsForIncompleteCasesCommand>
    {
        private readonly IQueryRepository<LegalEntityVendorRegistrationFormDto> _queryRepository;
        private readonly IEventPublisher _eventPublisher;

        private const string CompletedCaseStatus = "Case Request completed";

        public UpdateVrfCaseDetailsForIncompleteCasesCommandHandler(IQueryRepository<LegalEntityVendorRegistrationFormDto> queryRepository, IEventPublisher eventPublisher)
        {
            _queryRepository = queryRepository;
            _eventPublisher = eventPublisher;
        }

        public async Task Handle(UpdateVrfCaseDetailsForIncompleteCasesCommand command, CancellationToken cancellationToken = default)
        {
            var legalEntities = await _queryRepository.GetList(x => x.VrfCaseId != null && x.VrfCaseStatus != CompletedCaseStatus);
            var publishTasks = legalEntities.Select(x => _eventPublisher.Publish(new UpdateLegalEntityVrfCaseStatusEvent { LegalEntityId = x.LegalEntityId, VrfCaseId = x.VrfCaseId, VrfVendorId = x.VrfVendorId }));
            await Task.WhenAll(publishTasks);
        }
    }
}

