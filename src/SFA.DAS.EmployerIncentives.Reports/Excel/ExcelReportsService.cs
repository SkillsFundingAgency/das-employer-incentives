using SFA.DAS.EmployerIncentives.Reports.Models;
using SFA.DAS.EmployerIncentives.Reports.Reports.Metrics;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Reports.Excel
{
    public class ExcelReportsService : IExcelReports
    {     
        private readonly IReportsRepository _reportsRepository;

        public ExcelReportsService(IReportsRepository reportsRepository)
        {
            _reportsRepository = reportsRepository;
        }

        public Task<MetricsReport> GenerateMetricsReport()
        {
            return Task.FromResult(new MetricsReport
            {
                CollectionPeriod = new CollectionPeriod
                {
                    Period = 8,
                    AcademicYear = "2021"
                },
                PaymentsMade = new List<PaymentsMade>
                {
                    new PaymentsMade { Order = 1, Year = "2021", Period = 5, Number = 10, Amount = 10001.03 },
                    new PaymentsMade { Order = 2, Year = "2021", Period = 6, Number = 20, Amount = 10002.20 },
                    new PaymentsMade { Order = 3, Year = "2021", Period = 7, Number = 30, Amount = 10003.50 },
                    new PaymentsMade { Order = 4, Year = "2021", Period = 8, Number = 40, Amount = 8203.90 },
                    new PaymentsMade { Order = 5, Year = "2021", Period = 9, Number = 50, Amount = 10005.30 },
                    new PaymentsMade { Order = 6, Year = "2021", Period = 10, Number = 60, Amount = 10006.20 },
                    new PaymentsMade { Order = 7, Year = "2021", Period = 11, Number = 70, Amount = 10007.50 },
                    new PaymentsMade { Order = 8, Year = "2021", Period = 12, Number = 80, Amount = 10008.05 },
                    new PaymentsMade { Order = 9, Year = "2021", Period = 1, Number = 90, Amount = 10009.34 },
                    new PaymentsMade { Order = 10, Year = "2021", Period = 2, Number = 100, Amount = 10010.93 },
                    new PaymentsMade { Order = 11, Year = "2021", Period = 3, Number = 110, Amount = 10011.25 },
                    new PaymentsMade { Order = 12, Year = "2021", Period = 4, Number = 120, Amount = 10012.73 },
                    new PaymentsMade { Order = 13, Year = "2022", Period = 1, Number = 1, Amount = 1.0 }
                },
                Earnings = new List<Earning>
                {
                    new Earning { Order = 1, Year = "2021", Period = 3, Number = 10, Amount = 10001.00 },
                    new Earning { Order = 2, Year = "2021", Period = 4, Number = 20, Amount = 10002.01 },
                    new Earning { Order = 3, Year = "2021", Period = 5, Number = 30, Amount = 10003.34 },
                    new Earning { Order = 4, Year = "2021", Period = 6, Number = 40, Amount = 10004.76 },
                    new Earning { Order = 5, Year = "2021", Period = 7, Number = 50, Amount = 10005.06 },
                    new Earning { Order = 6, Year = "2021", Period = 8, Number = 60, Amount = 10006.77 },
                    new Earning { Order = 7, Year = "2021", Period = 9, Number = 70, Amount = 10007.35 },
                    new Earning { Order = 8, Year = "2021", Period = 10, Number = 80, Amount = 10008.52 },
                    new Earning { Order = 9, Year = "2021", Period = 11, Number = 90, Amount = 10009.19 },
                    new Earning { Order = 10, Year = "2021", Period = 12, Number = 100, Amount = 10010.23 },
                    new Earning { Order = 11, Year = "2022", Period = 1, Number = 110, Amount = 10011.05 },
                    new Earning { Order = 12, Year = "2022", Period = 2, Number = 120, Amount = 10012.40 },
                    new Earning { Order = 13, Year = "2022", Period = 3, Number = 130, Amount = 10013.60 },
                    new Earning { Order = 14, Year = "2022", Period = 4, Number = 140, Amount = 10014.53 },
                    new Earning { Order = 15, Year = "2022", Period = 5, Number = 150, Amount = 10015.36 },
                    new Earning { Order = 16, Year = "2022", Period = 6, Number = 160, Amount = 10016.00 },
                    new Earning { Order = 17, Year = "2022", Period = 7, Number = 170, Amount = 10017.00 },
                    new Earning { Order = 18, Year = "2022", Period = 8, Number = 180, Amount = 10018.00 },
                    new Earning { Order = 19, Year = "2022", Period = 9, Number = 190, Amount = 10019.00 },
                    new Earning { Order = 20, Year = "2022", Period = 10, Number = 200, Amount = 10020.00 },
                    new Earning { Order = 21, Year = "2022", Period = 11, Number = 210, Amount = 10021.00 },
                    new Earning { Order = 22, Year = "2022", Period = 12, Number = 220, Amount = 10022.00 },
                    new Earning { Order = 23, Year = "2023", Period = 1, Number = 230, Amount = 10023.00 },
                    new Earning { Order = 24, Year = "2023", Period = 2, Number = 240, Amount = 10024.00 }
                },
                ValidationSummary = new PeriodValidationSummary
                {
                    ValidRecords = new PeriodValidationSummary.ValidationSummaryRecord
                    {
                        Count = 1233433,
                        PeriodAmount = 10004.76,
                        YtdAmount = 34333331.12
                    },
                    InvalidRecords = new PeriodValidationSummary.ValidationSummaryRecord
                    {
                        Count = 87317,
                        PeriodAmount = 1100.23,
                        YtdAmount = 483633.45
                    }
                },
                Clawbacks = new Clawbacks
                {
                    Sent = -23599.45,
                    Unsent = -3000000.23
                },
                YtdValidations = new List<YtdValidation>
                {
                    new YtdValidation
                    {
                        CountOfPayments = 4,
                        HasLearningRecord = true,
                        IsInLearning = true,
                        HasDaysInLearning = true,
                        HasNoDataLocks = true,
                        HasBankDetails = true,
                        PaymentsNotPaused = true,
                        HasNoUnsentClawbacks = false,
                        HasIlrSubmission = true,
                        HasSignedMinVersion = true,
                        LearnerMatchSuccessful = true,
                        EmployedAtStartOfApprenticeship = false,
                        EmployedBeforeSchemeStarted = true,
                        NumberOfAccountLegalEntityIds = 2,
                        EarningAmount = 3000.00
                    },
                    new YtdValidation
                    {
                        CountOfPayments = 1,
                        NumberOfAccountLegalEntityIds = 4,
                        EarningAmount = 35000.00
                    }
                }
            }); 
        }

        public async Task Save(MetricsReport report)
        {
            using var ms = new MemoryStream();            
            var reportGenerator = new MetricsReportGenerator(report);
            reportGenerator.Create(ms);
            await _reportsRepository.Save(
                new ReportsFileInfo(report.Name, "xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Metrics"),
                ms);
        }
    }
}
