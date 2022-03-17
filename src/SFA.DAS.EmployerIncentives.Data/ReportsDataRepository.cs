using Dapper;
using Microsoft.Extensions.Options;
using SFA.DAS.EmployerIncentives.Data.Reports.Metrics;
using SFA.DAS.EmployerIncentives.Infrastructure.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using static Dapper.SqlMapper;

namespace SFA.DAS.EmployerIncentives.Data
{
    public interface IReportsDataRepository
    {
        Task<MetricsReport> Execute();
    }

    public class ReportsDataRepository : IReportsDataRepository
    {
        private readonly string _connectionString;

        public ReportsDataRepository(IOptions<ApplicationSettings> options)
        {
            _connectionString = options.Value.DbConnectionString;
        }

        public async Task<MetricsReport> Execute()
        {
            using var dbConnection = new SqlConnection(_connectionString);
            var results = await dbConnection.QueryMultipleAsync("reports.MetricsReport", commandType: System.Data.CommandType.StoredProcedure);

            return new MetricsReport
            {
                CollectionPeriod = results.Read<CollectionPeriod>().SingleOrDefault(),
                PaymentsMade = results.Read<PaymentsMade>().ToList(),
                Earnings = results.Read<Earning>().ToList(),
                Clawbacks = results.Read<Clawbacks>().SingleOrDefault(),
                PeriodValidations = results.Read<Validation>().ToList(),
                YtdValidations = results.Read<Validation>().ToList(),
                ValidationSummary = new PeriodValidationSummary
                {
                    ValidRecords = results.Read<PeriodValidationSummary.ValidationSummaryRecord>().SingleOrDefault(),
                    InvalidRecords = results.Read<PeriodValidationSummary.ValidationSummaryRecord>().SingleOrDefault()
                }
            };
        }      
    }
}
