using SFA.DAS.EmployerIncentives.Data.Reports.Metrics;
using System.Collections.Generic;
using System.Linq;

namespace SFA.DAS.EmployerIncentives.Reports.Reports.Metrics
{
    public class EarningsTable
    {
        private readonly Context _context;

        public EarningsTable(Context context)
        {
            _context = context;
        }

        public void Create(MetricsReport report)
        {
            AddSectionHeaderRow();
            AddHeaderRow();
            AddRows(report.Earnings);
            _context.RowNumber++;
        }

        private void AddSectionHeaderRow()
        {
            var currentRow = _context.Sheet.GetOrCreateRow(_context.RowNumber++);
            var cellNumber = _context.StartCellNumber;

            var cell = currentRow.CreateCell(cellNumber);
            cell.SetCellValue("Earnings");
            cell.CellStyle = _context.Styles[Style.Bold];
        }

        private void AddHeaderRow()
        {
            var currentRow = _context.Sheet.GetOrCreateRow(_context.RowNumber++);
            var cellNumber = _context.StartCellNumber;

            var cell = currentRow.CreateCell(cellNumber);
            cell.CellStyle = _context.Styles[Style.Bold];
            cell.SetCellValue("PeriodNumber");

            cell = currentRow.CreateCell(++cellNumber);
            cell.CellStyle = _context.Styles[Style.Bold];
            cell.SetCellValue("PaymentYear");

            cell = currentRow.CreateCell(++cellNumber);
            cell.CellStyle = _context.Styles[Style.Bold];
            cell.SetCellValue("Amount");

            cell = currentRow.CreateCell(++cellNumber);
            cell.CellStyle = _context.Styles[Style.Bold];
            cell.SetCellValue("NumEarnings");
        }

        private void AddRows(IEnumerable<Earning> earnings)
        {
            earnings.OrderBy(p => p.Order).ToList().ForEach(p => AddRow(p));
        }

        private void AddRow(Earning earning)
        {
            var currentRow = _context.Sheet.GetOrCreateRow(_context.RowNumber++);
            var cellNumber = _context.StartCellNumber;

            var cell = currentRow.CreateCell(cellNumber);
            cell.CellStyle = _context.Styles[Style.Default];
            cell.SetCellValue(earning.Period);

            cell = currentRow.CreateCell(++cellNumber);
            cell.CellStyle = _context.Styles[Style.Default];
            cell.SetCellValue(earning.Year);

            cell = currentRow.CreateCell(++cellNumber);
            cell.CellStyle = _context.Styles[Style.Default];
            cell.SetCellValue(earning.Amount);

            cell = currentRow.CreateCell(++cellNumber);
            cell.CellStyle = _context.Styles[Style.Default];
            cell.SetCellValue(earning.Number);
        }
    }
}
