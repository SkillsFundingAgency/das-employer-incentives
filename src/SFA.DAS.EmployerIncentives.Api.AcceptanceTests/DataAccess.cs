using Dapper;
using Dapper.Contrib.Extensions;
using SFA.DAS.EmployerIncentives.Data.Models;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Api.AcceptanceTests
{
    public class DataAccess
    {
        private readonly string _connectionString;

        public DataAccess(string connectionString)
        {
            _connectionString = connectionString;
        }

        public Account GetAccountByLegalEntityId(long legalEntityId)
        {
            using var dbConnection = new SqlConnection(_connectionString);
            var account = dbConnection.QuerySingle<Account>(
                "SELECT TOP 1 * FROM Accounts WHERE LegalEntityId = @LegalEntityId",
                new { legalEntityId });

            return account;
        }

        public Account GetAccountByAccountLegalEntityId(long accountId, long accountLegalEntityId)
        {
            using var dbConnection = new SqlConnection(_connectionString);
            var account = dbConnection.QuerySingle<Account>("SELECT * FROM Accounts WHERE Id = @accountId AND AccountLegalEntityId = @AccountLegalEntityId",
                new { accountId, accountLegalEntityId });

            return account;
        }

        public async Task Insert<T>(T entity) where T : class
        {
            await using var dbConnection = new SqlConnection(_connectionString);
            await dbConnection.InsertAsync(entity);
        }

        public async Task Update<T>(T entity) where T : class
        {
            await using var dbConnection = new SqlConnection(_connectionString);
            await dbConnection.UpdateAsync(entity);
        }

        public async Task SetupAccount(Account account)
        {
            await using var dbConnection = new SqlConnection(_connectionString);
            await dbConnection.InsertAsync(account);
        }

        // Use this method to add `Status` column data as string as dapper would insert numeric enum value
        public async Task InsertApplication(IncentiveApplication application)
        {
            await using var dbConnection = new SqlConnection(_connectionString);
            await dbConnection.ExecuteAsync(
                "insert into IncentiveApplication(id, accountId, accountLegalEntityId, dateCreated, status, dateSubmitted, submittedByEmail, submittedByName) values " +
                "(@id, @accountId, @accountLegalEntityId, @dateCreated, @status, @dateSubmitted, @submittedByEmail, @submittedByName)",
                new
                {
                    application.Id,
                    application.AccountId,
                    application.AccountLegalEntityId,
                    application.DateCreated,
                    Status = application.Status.ToString(),
                    application.DateSubmitted,
                    application.SubmittedByEmail,
                    application.SubmittedByName
                });
        }

    }
}
