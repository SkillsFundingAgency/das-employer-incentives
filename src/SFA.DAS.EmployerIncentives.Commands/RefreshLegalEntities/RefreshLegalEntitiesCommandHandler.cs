using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.Services;
using SFA.DAS.EmployerIncentives.Commands.Services.AccountApi;
using SFA.DAS.EmployerIncentives.Messages.Events;
using SFA.DAS.NServiceBus.Services;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Commands.RefreshLegalEntities
{
    public class RefreshLegalEntitiesCommandHandler : ICommandHandler<RefreshLegalEntitiesCommand>
    {
        private readonly IEventPublisher _eventPublisher;
        private readonly IAccountService _accountService;
        private readonly IMultiEventPublisher _multiEventPublisher;        

        public RefreshLegalEntitiesCommandHandler(
            IEventPublisher eventPublisher,
            IAccountService accountService,
            IMultiEventPublisher multiEventPublisher)
        {
            _eventPublisher = eventPublisher;
            _accountService = accountService;
            _multiEventPublisher = multiEventPublisher;
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
            var messages = new List<RefreshLegalEntitiesEvent>();

            for (int i = 2; i <= totalPages; i++)
            {
                messages.Add(new RefreshLegalEntitiesEvent { PageNumber = i, PageSize = pageSize });
            }
            
            await _multiEventPublisher.Publish(messages);
        }

        private async Task ProcessLegalEntities(List<AccountLegalEntity> accountLegalEntities)
        {
            var messages = new List<RefreshLegalEntityEvent>();

            foreach (var accountLegalEntity in accountLegalEntities)
            {
                var legalEntityResponse = await _accountService.GetLegalEntity(accountLegalEntity.AccountId, accountLegalEntity.LegalEntityId);

                messages.Add(new RefreshLegalEntityEvent
                {
                    AccountId = accountLegalEntity.AccountId,
                    AccountLegalEntityId = accountLegalEntity.AccountLegalEntityId,
                    LegalEntityId = accountLegalEntity.LegalEntityId,
                    OrganisationName = legalEntityResponse.LegalEntity.Name
                });
            }

            await _multiEventPublisher.Publish(messages);

        }
    }
}

