using SFA.DAS.EmployerIncentives.Reports.Models;

namespace SFA.DAS.EmployerIncentives.Reports.Reports.Metrics
{
    public class CollectionPeriodTable
    {
        private readonly Context _context;

        public CollectionPeriodTable(Context context)
        {
            _context = context;
        }

        public void Create(MetricsReport report)
        {
            AddCurrentPeriodRow(report.CollectionPeriod);
            AddCurrentYearRow(report.CollectionPeriod);
            _context.RowNumber++;
        }

        private void AddCurrentPeriodRow(CollectionPeriod period)
        {
            var currentRow = _context.Sheet.GetOrCreateRow(_context.RowNumber++);

            var cellNumber = _context.StartCellNumber;

            var cell = currentRow.CreateCell(cellNumber);
            cell.SetCellValue("Current Period");

            cell = currentRow.CreateCell(++cellNumber);
            cell.SetCellValue(period.Period);
            cell.CellStyle = _context.Styles[Style.Default];
        }

        private void AddCurrentYearRow(CollectionPeriod period)
        {
            var currentRow = _context.Sheet.GetOrCreateRow(_context.RowNumber++);
            var cellNumber = _context.StartCellNumber; 

            var cell = currentRow.CreateCell(cellNumber);
            cell.SetCellValue("Current Year");

            cell = currentRow.CreateCell(++cellNumber);
            cell.CellStyle = _context.Styles[Style.Default];
            cell.SetCellValue(period.AcademicYear);
        }
    }
}
