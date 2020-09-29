using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.Services.AccountApi;
using SFA.DAS.EmployerIncentives.Messages.Events;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.NServiceBus.Services;

namespace SFA.DAS.EmployerIncentives.Commands.RefreshLegalEntities
{
    public class RefreshLegalEntitiesCommandHandler : ICommandHandler<RefreshLegalEntitiesCommand>
    {
        private readonly IAccountService _accountService;
        private readonly IEventPublisher _eventPublisher;

        public RefreshLegalEntitiesCommandHandler(
            IAccountService accountService,
            IEventPublisher eventPublisher)
        {
            _accountService = accountService;
            _eventPublisher = eventPublisher;
        }
        public async Task Handle(RefreshLegalEntitiesCommand command, CancellationToken cancellationToken = default)
        {
            var response = await _accountService.GetAccountLegalEntitiesByPage(command.PageNumber, command.PageSize);

            await ProcessLegalEntities(response.Data);

            if (response.Page == 1)
            {
                await ProcessOtherPages(response.TotalPages, command.PageSize);
            }
        }

        private async Task ProcessOtherPages(int totalPages, int pageSize)
        {
            for (int i = 2; i <= totalPages; i++)
            {
                await _eventPublisher.Publish(new RefreshLegalEntitiesEvent { PageNumber = i, PageSize = pageSize});
            }
        }

        private async Task ProcessLegalEntities(List<AccountLegalEntity> accountLegalEntities)
        {
            var tasks = accountLegalEntities.Select(ale=> _eventPublisher.Publish(new RefreshLegalEntityEvent
                {
                    AccountId = ale.AccountId,
                    AccountLegalEntityId = ale.AccountLegalEntityId,
                    LegalEntityId = ale.LegalEntityId,
                    OrganisationName = ale.Name
                }));

            await Task.WhenAll(tasks);
        }
    }
}