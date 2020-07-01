using Dapper;
using Dapper.Contrib.Extensions;
using Microsoft.Extensions.Options;
using SFA.DAS.EmployerIncentives.Data.Map;
using SFA.DAS.EmployerIncentives.Data.Tables;
using SFA.DAS.EmployerIncentives.Domain.Accounts.Models;
using SFA.DAS.EmployerIncentives.Infrastructure.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;

namespace SFA.DAS.EmployerIncentives.Data
{
    public class AccountDataRepository : IAccountDataRepository
    {
        private readonly string _dbConnectionString;
        public AccountDataRepository(IOptions<ApplicationSettings> options)
        {
            _dbConnectionString = options?.Value.DbConnectionString;
        }

        public async Task Update(AccountModel account)
        {
            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            using (var dbConnection = new SqlConnection(_dbConnectionString))
            {
                await dbConnection.OpenAsync();
                var existing = await dbConnection.QueryAsync<AccountTable>("SELECT * FROM Accounts WHERE Id = @Id", new { account.Id });

                foreach (var item in existing)
                {
                    if(!account.LegalEntityModels.Any(i => i.AccountLegalEntityId == item.AccountLegalEntityId))
                    {
                        await dbConnection.DeleteAsync(item);
                    }
                }

                foreach (var item in account.Map())
                {
                    await dbConnection.ExecuteAsync(
                        "AddOrUpdateAccount",
                        new { item.Id, item.AccountLegalEntityId, item.LegalEntityId, item.LegalEntityName },
                        commandType: CommandType.StoredProcedure);
                }

                scope.Complete();
            }
        }

        public async Task Add(AccountModel account)
        {
            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            using (var dbConnection = new SqlConnection(_dbConnectionString))
            {
                await dbConnection.OpenAsync();
                await dbConnection.InsertAsync(account.Map());
                scope.Complete();
            }
        }

        public async Task<AccountModel> Find(long accountId)
        {
            using (var dbConnection = new SqlConnection(_dbConnectionString))
            {
                var account = await dbConnection.QueryAsync<AccountTable>("SELECT * FROM Accounts WHERE Id = @Id", new { Id = accountId });
                return account?.MapSingle();                
            }
        }
    }
}
