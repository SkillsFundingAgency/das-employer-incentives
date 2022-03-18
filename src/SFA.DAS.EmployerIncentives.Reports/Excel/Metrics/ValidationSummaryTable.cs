using SFA.DAS.EmployerIncentives.Data.Reports.Metrics;

namespace SFA.DAS.EmployerIncentives.Reports.Excel.Metrics
{
    public class ValidationSummaryTable
    {
        private readonly Context _context;

        public ValidationSummaryTable(Context context)
        {
            _context = context;
        }

        public void Create(MetricsReport report)
        {
            AddHeaderRow(report.CollectionPeriod);
            AddValidRow(report.ValidationSummary);
            AddInValidRow(report.ValidationSummary);
            AddTotalRow(report.ValidationSummary);
            _context.RowNumber++;
        }

        private void AddHeaderRow(CollectionPeriod collectionPeriod)
        {
            var currentRow = _context.Sheet.GetOrCreateRow(_context.RowNumber++);
            var cellNumber = _context.StartCellNumber;

            var cell = currentRow.CreateCell(cellNumber++);
            cell.CellStyle = _context.Styles[Style.Bold];
            cell.SetCellValue("Validation");
            cell.AddComment("This shows the last validation record status of each earning.  If the last validation has passed and the earning is due then it should be paid.");

            cellNumber++;

            cell = currentRow.CreateCell(cellNumber++);
            cell.CellStyle = _context.Styles[Style.Bold];
            cell.SetCellValue("Count");

            cell = currentRow.CreateCell(cellNumber++);
            cell.CellStyle = _context.Styles[Style.Bold];
            cell.SetCellValue($"R{collectionPeriod.Period}");
            cell.AddComment("Shows only this months validation record status.");

            cell = currentRow.CreateCell(cellNumber);
            cell.CellStyle = _context.Styles[Style.Bold];
            cell.SetCellValue("YTD");
            cell.AddComment("Shows only the validation status for all records for the academic year to date.");
        }

        private void AddValidRow(PeriodValidationSummary validationSummary)
        {
            var currentRow = _context.Sheet.GetOrCreateRow(_context.RowNumber++);
            var cellNumber = _context.StartCellNumber;

            var cell = currentRow.CreateCell(cellNumber++);
            cell.CellStyle = _context.Styles[Style.Default];
            cell.SetCellValue("Valid records");

            cellNumber++;

            cell = currentRow.CreateCell(cellNumber++);
            cell.CellStyle = _context.Styles[Style.Default];
            cell.SetCellValue(validationSummary.ValidRecords.Count);

            cell = currentRow.CreateCell(cellNumber++);
            cell.CellStyle = _context.Styles[Style.Default];
            cell.SetCellValue(validationSummary.ValidRecords.PeriodAmount);

            cell = currentRow.CreateCell(cellNumber);
            cell.CellStyle = _context.Styles[Style.Default];
            cell.SetCellValue(validationSummary.ValidRecords.YtdAmount);
        }

        private void AddInValidRow(PeriodValidationSummary validationSummary)
        {
            var currentRow = _context.Sheet.GetOrCreateRow(_context.RowNumber++);
            var cellNumber = _context.StartCellNumber;

            var cell = currentRow.CreateCell(cellNumber++);
            cell.CellStyle = _context.Styles[Style.Default];
            cell.SetCellValue("Invalid records");

            cellNumber++;

            cell = currentRow.CreateCell(cellNumber++);
            cell.CellStyle = _context.Styles[Style.Default];
            cell.SetCellValue(validationSummary.InvalidRecords.Count);

            cell = currentRow.CreateCell(cellNumber++);
            cell.CellStyle = _context.Styles[Style.Default];
            cell.SetCellValue(validationSummary.InvalidRecords.PeriodAmount);

            cell = currentRow.CreateCell(cellNumber);
            cell.CellStyle = _context.Styles[Style.Default];
            cell.SetCellValue(validationSummary.InvalidRecords.YtdAmount);
        }

        private void AddTotalRow(PeriodValidationSummary validationSummary)
        {
            var currentRow = _context.Sheet.GetOrCreateRow(_context.RowNumber++);
            var cellNumber = _context.StartCellNumber + 2;

            var cell = currentRow.CreateCell(cellNumber);
            cell.SetCellValue("Total Validated");
            cell.CellStyle = _context.Styles[Style.Bold];

            cellNumber++;

            cell = currentRow.CreateCell(cellNumber++);
            cell.CellStyle = _context.Styles[Style.CurrencyBold];
            cell.SetCellValue(validationSummary.ValidRecords.PeriodAmount + validationSummary.InvalidRecords.PeriodAmount);

            cell = currentRow.CreateCell(cellNumber);
            cell.CellStyle = _context.Styles[Style.CurrencyBold];
            cell.SetCellValue(validationSummary.ValidRecords.YtdAmount + validationSummary.InvalidRecords.YtdAmount);
        }
    }
}
