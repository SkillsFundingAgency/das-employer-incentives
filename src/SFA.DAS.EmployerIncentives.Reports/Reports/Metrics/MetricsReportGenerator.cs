using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using SFA.DAS.EmployerIncentives.Reports.Models;
using System.Collections.Generic;
using System.IO;

namespace SFA.DAS.EmployerIncentives.Reports.Reports.Metrics
{
    public class MetricsReportGenerator
    {
        private readonly XSSFWorkbook _workbook;
        private readonly IDictionary<Style, ICellStyle> _styles;
        private readonly MetricsReport _metricsReport;

        public MetricsReportGenerator(MetricsReport metricsReport)
        {
            _workbook = new XSSFWorkbook();
            _metricsReport = metricsReport;
            _styles = CreateStyles(_workbook);
        }

        public void Create(Stream stream)
        {
            CreateMainSheet(_workbook, _metricsReport, _styles);
            CreateYTDSheet(_workbook, _metricsReport, _styles);
            _workbook.Write(stream, true);
        }

        private static void CreateMainSheet(IWorkbook workbook, MetricsReport report, IDictionary<Style, ICellStyle> styles)
        {
            var mainSheet = workbook.CreateSheet("Main");

            var context = new Context(mainSheet, styles);

            new CollectionPeriodTable(context).Create(report);
            new PaymentsMadeTable(context).Create(report);
            new ValidationSummaryTable(context).Create(report);
            new EarningsSummaryTable(context).Create(report);
            new AccountedForEarningsTable(context).Create(report);
            new ClawbacksSummaryTable(context).Create(report);            
            new EarningsTable(new Context(mainSheet, styles, 3, 6)).Create(report);

            mainSheet.SetColumnWidths(0, 9, 20);
        }

        private static void CreateYTDSheet(IWorkbook workbook, MetricsReport report, IDictionary<Style, ICellStyle> styles)
        {
            var ytdValidationSheet = workbook.CreateSheet("YTD Validation");
            new YtdValidationTable(new Context(ytdValidationSheet, styles)).Create(report);

            ytdValidationSheet.SetColumnWidths(0, 11, 22);
        }

        private static IDictionary<Style, ICellStyle> CreateStyles(IWorkbook workbook)
        {
            var styles = new Dictionary<Style, ICellStyle>();

            var defaultStyle = workbook.CreateCellStyle();
            defaultStyle.Alignment = HorizontalAlignment.Right;
            styles.Add(Style.Default, defaultStyle);

            var boldFont = workbook.CreateFont();
            boldFont.IsBold = true;
            var boldStyle = workbook.CreateCellStyle();
            boldStyle.SetFont(boldFont);
            styles.Add(Style.Bold, boldStyle);

            // To Get the built in data formats use HSSFDataFormat.GetBuiltinFormats();
            var currencyStyle = workbook.CreateCellStyle();
            currencyStyle.DataFormat = HSSFDataFormat.GetBuiltinFormat("\"$\"#,##0.00_);(\"$\"#,##0.00)");

            styles.Add(Style.Currency, currencyStyle);

            var percentageStyle = workbook.CreateCellStyle();
            percentageStyle.DataFormat = HSSFDataFormat.GetBuiltinFormat("0.00%");
            styles.Add(Style.Percentage, percentageStyle);

            var currencyBoldStyle = workbook.CreateCellStyle();
            currencyBoldStyle.SetFont(boldFont);
            currencyBoldStyle.DataFormat = HSSFDataFormat.GetBuiltinFormat("\"$\"#,##0.00_);(\"$\"#,##0.00)");
            styles.Add(Style.CurrencyBold, currencyBoldStyle);

            return styles;
        }
    }
}
