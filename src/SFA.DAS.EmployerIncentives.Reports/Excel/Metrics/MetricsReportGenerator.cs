using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using SFA.DAS.EmployerIncentives.Data.Reports.Metrics;
using System;
using System.Collections.Generic;
using System.IO;

namespace SFA.DAS.EmployerIncentives.Reports.Excel.Metrics
{
    public class MetricsReportGenerator : IExcelReportGenerator<MetricsReport>
    {
        private readonly XSSFWorkbook _workbook;
        private readonly IDictionary<Style, ICellStyle> _styles;
        private readonly MemoryStream _ms;
        private bool _isDisposed;

        public MetricsReportGenerator()
        {
            _workbook = new XSSFWorkbook();
            _styles = CreateStyles(_workbook);
            _ms = new MemoryStream();
        }

        public Stream Create(MetricsReport report)
        {
            CreateMainSheet(_workbook, report, _styles);
            CreateYTDSheet(_workbook, report, _styles);

            _workbook.Write(_ms, true);
            return _ms;
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
            var validationSheet = workbook.CreateSheet("YTD Validation");
            var context = new Context(validationSheet, styles);

            new ValidationTable(context).Create(report);
            context.RowNumber++;
            new ValidationTable(context).Create(report, true);

            validationSheet.SetColumnWidths(0, 11, 22);
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

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    _ms.Dispose();
                }

                _isDisposed = true;
            }
        }
    }
}
