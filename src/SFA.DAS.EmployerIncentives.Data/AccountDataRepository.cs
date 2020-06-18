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
        private readonly IDbConnection _dbConnection;
        public AccountDataRepository(IOptions<FunctionSettings> options)
        {
            _dbConnection = new SqlConnection(options?.Value.DbConnectionString);
        }

        public async Task Add(IAccountModel account)
        {
            await _dbConnection.InsertAsync<Account>(account.Map()); 
        }

        public async Task<IAccountModel> Find(long accountId)
        {
            var account = await _dbConnection.QueryFirstOrDefaultAsync<Account>("SELECT * FROM Accounts WHERE Id = @Id", new { Id = accountId });
            return account?.Map();
        }
    }
}
