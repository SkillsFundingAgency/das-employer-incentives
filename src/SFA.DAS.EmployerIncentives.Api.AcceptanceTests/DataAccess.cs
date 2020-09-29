using Dapper;
using SFA.DAS.EmployerIncentives.Data.Models;
using System.Data.SqlClient;

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

        public void SetupAccount(Account account)
        {
            using var dbConnection = new SqlConnection(_connectionString);
            dbConnection.Execute(
                "insert into Accounts(id, accountLegalEntityId, legalEntityId, hashedLegalEntityId, legalEntityName, hasSignedIncentivesTerms, vrfCaseId, vrfVendorId, vrfCaseStatus, vrfCaseStatusLastUpdatedDateTime) values " +
                "(@id, @accountLegalEntityId, @legalEntityId, @hashedLegalEntityId, @legalEntityName, @hasSignedIncentivesTerms, @vrfCaseId, @vrfVendorId, @vrfCaseStatus, @vrfCaseStatusLastUpdatedDateTime)", account);
        }

        public void SetupApplication(IncentiveApplication application)
        {
            using var dbConnection = new SqlConnection(_connectionString);
            dbConnection.Execute(
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

        public void SetupApprenticeship(IncentiveApplicationApprenticeship apprenticeship)
        {
            using var dbConnection = new SqlConnection(_connectionString);
            dbConnection.Execute(
                "insert into IncentiveApplicationApprenticeship(id, incentiveApplicationId, apprenticeshipId, firstName, lastName, dateOfBirth, " +
                "uln, plannedStartDate, apprenticeshipEmployerTypeOnApproval, TotalIncentiveAmount) values " +
                "(@id, @incentiveApplicationId, @apprenticeshipId, @firstName, @lastName, @dateOfBirth, " +
                "@uln, @plannedStartDate, @apprenticeshipEmployerTypeOnApproval, @totalIncentiveAmount)",
                apprenticeship);
        }
    }
}
