using Dapper;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.UpdateUKPRN
{
    public class EmployerIncentiveRepository
    {
        private readonly string _connectionString;
        public EmployerIncentiveRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<IEnumerable<long>> GetApprenticeshipsWithoutUKPRN()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                if (connection.State != ConnectionState.Open)
                {
                    await connection.OpenAsync();
                }

                var sql = "SELECT ApprenticeshipId FROM dbo.IncentiveApplicationApprenticeship WHERE UKPRN IS NULL";

                var apprenticeshipIds = await connection.QueryAsync<long>(sql);

                return await Task.FromResult(apprenticeshipIds);
            }
        }

        public string GenerateUKPRNUpdateScript(IEnumerable<ApprenticeshipUKPRN> apprenticeshipUKPRNs)
        {
            var sqlScript = string.Empty;
            foreach (var apprenticeshipUkprn in apprenticeshipUKPRNs)
            {
                var sql = $"UPDATE dbo.IncentiveApplicationApprenticeship SET UKPRN = {apprenticeshipUkprn.UKPRN} WHERE ApprenticeshipId = {apprenticeshipUkprn.ApprenticeshipId}\r\n";

                sqlScript += sql;
            }

            return sqlScript;
        }
    }
}
