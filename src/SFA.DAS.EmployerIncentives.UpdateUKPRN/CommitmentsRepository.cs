using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.UpdateUKPRN
{
    public class CommitmentsRepository
    {
        private readonly string _connectionString;
        public CommitmentsRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<IEnumerable<ApprenticeshipUKPRN>> GetUKPRNsForApprenticeships(IEnumerable<long> apprenticeshipIds)
        {
            var idList = String.Join(",", apprenticeshipIds);
            using (var connection = new SqlConnection(_connectionString))
            {
                if (connection.State != ConnectionState.Open)
                {
                    await connection.OpenAsync();
                }
                var sql = $"SELECT a.Id AS [ApprenticeshipId], c.ProviderId AS [UKPRN] from dbo.Apprenticeship a INNER JOIN dbo.Commitment c ON a.CommitmentId = c.Id WHERE a.Id IN({idList})";

                var apprenticeshipUkprns = await connection.QueryAsync<ApprenticeshipUKPRN>(sql);

                return apprenticeshipUkprns;
            }
        }
    }
}
