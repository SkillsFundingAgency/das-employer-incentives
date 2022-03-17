using SFA.DAS.EmployerIncentives.Data.Reports.Metrics;

namespace SFA.DAS.EmployerIncentives.Reports.Reports.Metrics
{
    public class ClawbacksSummaryTable
    {
        private readonly Context _context;

        public ClawbacksSummaryTable(Context context)
        {
            _context = context;
        }

        public void Create(MetricsReport report)
        {
            AddSentRow(report.Clawbacks);
            AddUnSentRow(report.Clawbacks);
            AddTotalRow(report.Clawbacks);
            _context.RowNumber++;
        }

        private void AddSentRow(Clawbacks clawbacks)
        {
            var currentRow = _context.Sheet.GetOrCreateRow(_context.RowNumber++);
            var cellNumber = _context.StartCellNumber;

            var cell = currentRow.CreateCell(cellNumber);
            cell.CellStyle = _context.Styles[Style.Bold];
            cell.SetCellValue("Clawbacks");

            cellNumber += 2;

            cell = currentRow.CreateCell(cellNumber++);
            cell.CellStyle = _context.Styles[Style.Default];
            cell.SetCellValue("Sent");

            cell = currentRow.CreateCell(cellNumber);
            cell.CellStyle = _context.Styles[Style.Default];
            cell.SetCellValue(clawbacks.Sent);
        }

        private void AddUnSentRow(Clawbacks clawbacks)
        {
            var currentRow = _context.Sheet.GetOrCreateRow(_context.RowNumber++);
            var cellNumber = _context.StartCellNumber + 2;

            var cell = currentRow.CreateCell(cellNumber++);
            cell.CellStyle = _context.Styles[Style.Default];
            cell.SetCellValue("Unsent");

            cell = currentRow.CreateCell(cellNumber);
            cell.CellStyle = _context.Styles[Style.Default];
            cell.SetCellValue(clawbacks.Unsent);
        }

        private void AddTotalRow(Clawbacks clawbacks)
        {
            var currentRow = _context.Sheet.GetOrCreateRow(_context.RowNumber++);
            var cellNumber = _context.StartCellNumber + 2;

            var cell = currentRow.CreateCell(cellNumber++);
            cell.CellStyle = _context.Styles[Style.Bold];
            cell.SetCellValue("Total");

            cell = currentRow.CreateCell(cellNumber);
            cell.CellStyle = _context.Styles[Style.CurrencyBold];
            cell.SetCellValue(clawbacks.Sent + clawbacks.Unsent);
        }
    }
}
