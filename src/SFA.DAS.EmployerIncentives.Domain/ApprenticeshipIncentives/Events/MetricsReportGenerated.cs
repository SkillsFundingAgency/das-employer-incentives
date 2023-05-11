using SFA.DAS.EmployerIncentives.Abstractions.Logging;
using SFA.DAS.EmployerIncentives.Domain.IncentiveApplications;
using SFA.DAS.EmployerIncentives.Domain.ValueObjects;

namespace SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Events
{
    public class MetricsReportGenerated : IDomainEvent, ILogWriter
    {
        public CollectionPeriod CollectionPeriod { get; }

        public MetricsReportGenerated(
            CollectionPeriod collectionPeriod)
        {
            CollectionPeriod = collectionPeriod;
        }

        public Log Log
        {
            get
            {
                var message =
                    $"Metrics report has been generated for PeriodNumber  {CollectionPeriod.PeriodNumber} and " +
                    $"Academic year {CollectionPeriod.AcademicYear}" ;
                return new Log
                {
                    OnProcessing = () => message,
                    OnError = () => message
                };
            }
        }
    }
}
