﻿using System;
using System.Collections.Generic;
using Dapper;
using Dapper.Contrib.Extensions;
using SFA.DAS.EmployerIncentives.Data.Models;
using System.Data.SqlClient;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives.Models;

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

        public IEnumerable<Account> GetAccountsById(long accountId)
        {
            using var dbConnection = new SqlConnection(_connectionString);
            var accounts = dbConnection.Query<Account>("SELECT * FROM Accounts WHERE Id = @accountId", new { accountId });

            return accounts;
        }

        public IEnumerable<PendingPayment> GetPendingPayments(Guid apprenticeshipIncentiveId)
        {
            using var dbConnection = new SqlConnection(_connectionString);
            var pendingPayments = dbConnection.Query<PendingPayment>("SELECT * FROM incentives.PendingPayment WHERE ApprenticeshipIncentiveId = @apprenticeshipIncentiveId",
                new { apprenticeshipIncentiveId });

            return pendingPayments;
        }

        public async Task Insert<T>(T entity) where T : class
        {
            await using var dbConnection = new SqlConnection(_connectionString);
            await dbConnection.InsertAsync(entity, false);
        }

        public async Task Update<T>(T entity) where T : class
        {
            await using var dbConnection = new SqlConnection(_connectionString);
            await dbConnection.UpdateAsync(entity);
        }

        public async Task SetupAccount(Account account)
        {
            await using var dbConnection = new SqlConnection(_connectionString);
            await dbConnection.InsertAsync(account, false);
        }

        public async Task<long> InsertWithEnumAsString<T>(T entity) where T : class
        {
            await using var dbConnection = new SqlConnection(_connectionString);
            return await dbConnection.InsertAsync(entity, true);
        }
    }
}
