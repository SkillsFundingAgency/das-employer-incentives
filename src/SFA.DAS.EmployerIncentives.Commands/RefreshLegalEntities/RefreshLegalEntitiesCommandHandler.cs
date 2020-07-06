using Newtonsoft.Json;
using Polly;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Messages.Events;
using SFA.DAS.HashingService;
using SFA.DAS.NServiceBus.Services;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Commands.RefreshLegalEntities
{
    public class RefreshLegalEntitiesCommandHandler
    {
        public class AccountLegalEntity
        {
            public long AccountId { get; set; }
            public long AccountLegalEntityId { get; set; }
            public long LegalEntityId { get; set; }
        }

        public class LegalEntity
        {
            public long LegalEntityId { get; set; }
            public string Name { get; set; }
        }

        public class PagedModel<T>
        {
            public List<T> Data { get; set; }
            public int Page { get; set; }
            public int TotalPages { get; set; }
        }

        public interface IAccountService
        {
            Task<PagedModel<AccountLegalEntity>> GetAccountLegalEntitiesByPage(int pageNumber, int pageSize = 1000);
            Task<LegalEntity> GetLegalEntity(long accountId, long legalentityId);
        }

        public class AccountService : IAccountService
        {
            private readonly HttpClient _client;
            private readonly IHashingService _hashingService;

            public AccountService(
                HttpClient client,
                IHashingService hashingService)
            {
                _client = client;
                _hashingService = hashingService;
            }

            public async Task<PagedModel<AccountLegalEntity>> GetAccountLegalEntitiesByPage(int pageNumber, int pageSize = 1000)
            {
                var response = await _client.GetAsync($"api/accountlegalentities?pageNumber={pageNumber}&pageSize={pageSize}");

                response.EnsureSuccessStatusCode();

                return JsonConvert.DeserializeObject<PagedModel<AccountLegalEntity>>(await response.Content.ReadAsStringAsync());
            }

            public async Task<LegalEntity> GetLegalEntity(long accountId, long legalentityId)
            {
                var hashedAccountId = _hashingService.HashValue(accountId);

                var response = await _client.GetAsync($"api/accounts/{hashedAccountId}/legalEntities/{legalentityId}");

                response.EnsureSuccessStatusCode();

                return JsonConvert.DeserializeObject<LegalEntity>(await response.Content.ReadAsStringAsync());
            }

            public class RefreshLegalEntitiesCommandHandler : ICommandHandler<RefreshLegalEntitiesCommand>
            {
                private readonly IEventPublisher _eventPublisher;
                private readonly IAccountService _accountService;

                public RefreshLegalEntitiesCommandHandler(
                    IEventPublisher eventPublisher,
                    IAccountService accountService)
                {
                    _eventPublisher = eventPublisher;
                    _accountService = accountService;
                }

                public async Task Handle(RefreshLegalEntitiesCommand command, CancellationToken cancellationToken = default)
                {
                    var response = await _accountService.GetAccountLegalEntitiesByPage(command.PageNumber, command.PageSize);
                    //cancellationToken.ThrowIfCancellationRequested();

                    if (response.Page == 1)
                    {
                        var tasksToRun = new List<Task>();
                        var policy = Policy.BulkheadAsync(5, response.TotalPages);

                        for (int i = 2; i <= response.TotalPages; i++)
                        {
                            tasksToRun.Add(policy.ExecuteAndCaptureAsync(() => _eventPublisher.Publish(new RefreshLegalEntitiesEvent { PageNumber = i, PageSize = command.PageSize })));
                        }

                        await Task.WhenAll(tasksToRun);
                        foreach (var task in tasksToRun)
                        {
                            await task;
                        }
                    }

                    foreach (var accountLegalEntity in response.Data)
                    {
                        //cancellationToken.ThrowIfCancellationRequested();

                        var legalEntityResponse = await _accountService.GetLegalEntity(accountLegalEntity.AccountId, accountLegalEntity.LegalEntityId);

                        await _eventPublisher.Publish(new RefreshLegalEntityEvent
                        {
                            AccountId = accountLegalEntity.AccountId,
                            AccountLegalEntityId = accountLegalEntity.AccountLegalEntityId,
                            LegalEntityId = accountLegalEntity.LegalEntityId,
                            OrganisationName = legalEntityResponse.Name
                        });
                    }

                }

            }
        }
    }
}
