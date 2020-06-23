using Dapper;
using Dapper.Contrib.Extensions;
using Microsoft.Extensions.Options;
using SFA.DAS.EmployerIncentives.Data.Tables;
using SFA.DAS.EmployerIncentives.Domain.Interfaces;
using SFA.DAS.EmployerIncentives.Infrastructure.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Data.Map;

namespace SFA.DAS.EmployerIncentives.Data
{
    public class AccountDataRepository : IAccountDataRepository
    {
        private readonly string _dbConnectionString;
        public AccountDataRepository(IOptions<ApplicationSettings> options)
        {
            _dbConnectionString = options?.Value.DbConnectionString;
        }

        public async Task Update(IAccountModel account)
        {
            using (var dbConnection = new SqlConnection(_dbConnectionString))
            {
                await dbConnection.OpenAsync();
                using (var transaction = dbConnection.BeginTransaction())
                {
                    try
                    {
                        foreach (var item in account.Map())
                        {
                            await dbConnection.ExecuteAsync(
                                "AddOrUpdateAccount",
                                new { item.Id, item.AccountLegalEntityId, item.LegalEntityId, item.LegalEntityName },
                                transaction: transaction,
                                commandType: CommandType.StoredProcedure);
                        }
                        transaction.Commit();
                        dbConnection.Close();
                    }

                    catch
                    {
                        transaction.Rollback();
                    }
                }
            }
        }

        public async Task Add(IAccountModel account)
        {
            using (var dbConnection = new SqlConnection(_dbConnectionString))
            {
                await dbConnection.InsertAsync(account.Map());
            }
        }

        public async Task<IAccountModel> Find(long accountId)
        {
            using (var dbConnection = new SqlConnection(_dbConnectionString))
            {
                var account = await dbConnection.QueryAsync<Account>("SELECT * FROM Accounts WHERE Id = @Id", new { Id = accountId });
                return account?.MapSingle();
            }
        }
    }
}
