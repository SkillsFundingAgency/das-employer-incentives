using Dapper;
using SFA.DAS.EmployerIncentives.Data.Reports.Metrics;
using System.Linq;
using System.Threading.Tasks;
using static Dapper.SqlMapper;

namespace SFA.DAS.EmployerIncentives.Data.Reports
{
    public class ReportsDataRepository : IReportsDataRepository
    {
        private readonly IReportsConnectionProvider _connectionProvider;

        public ReportsDataRepository(IReportsConnectionProvider connectionProvider)
        {
            _connectionProvider = connectionProvider;
        }

        public async Task<T> Execute<T>() where T : class, IReport
        {
            if (typeof(T) != typeof(MetricsReport))
            {
                return default;
            }

            using var dbConnection = _connectionProvider.New();
            var results = await dbConnection.QueryMultipleAsync("reports.MetricsReport", commandType: System.Data.CommandType.StoredProcedure);

            var report = new MetricsReport
            {
                CollectionPeriod = results.Read<CollectionPeriod>().SingleOrDefault(),
                PaymentsMade = results.Read<PaymentsMade>().ToList(),
                Earnings = results.Read<Earning>().ToList(),
                Clawbacks = results.Read<Clawbacks>().SingleOrDefault(),
                PeriodValidations = results.Read<Validation>().ToList(),                
                ValidationSummary = new PeriodValidationSummary
                {
                    ValidRecords = results.Read<PeriodValidationSummary.ValidationSummaryRecord>().SingleOrDefault(),
                    InvalidRecords = results.Read<PeriodValidationSummary.ValidationSummaryRecord>().SingleOrDefault()
                }
            };

            return report as T;
        }
    }
}
