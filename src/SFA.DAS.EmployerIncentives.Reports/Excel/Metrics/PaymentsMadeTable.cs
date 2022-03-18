using SFA.DAS.EmployerIncentives.Data.Reports.Metrics;
using System.Collections.Generic;
using System.Linq;

namespace SFA.DAS.EmployerIncentives.Reports.Excel.Metrics
{
    public class PaymentsMadeTable
    {
        private readonly Context _context;

        public PaymentsMadeTable(Context context)
        {
            _context = context;            
        }

        public void Create(MetricsReport report)
        {
            AddSectionHeaderRow();
            AddHeaderRow();
            AddRows(report.PaymentsMade);
            AddTotalRow(report.PaymentsMade);
            AddPercentagePaidRow(report.CollectionPeriod, report.PaymentsMade, report.ValidationSummary);
            _context.RowNumber++;
        }

        private void AddSectionHeaderRow()
        {
            var currentRow = _context.Sheet.GetOrCreateRow(_context.RowNumber++);
            var cellNumber = _context.StartCellNumber;

            var cell = currentRow.CreateCell(cellNumber);
            cell.SetCellValue("Payments Made");
            cell.CellStyle = _context.Styles[Style.Bold];
        }

        private void AddHeaderRow()
        {
            var currentRow = _context.Sheet.GetOrCreateRow(_context.RowNumber++);
            var cellNumber = _context.StartCellNumber;

            var cell = currentRow.CreateCell(cellNumber);
            cell.CellStyle = _context.Styles[Style.Bold];
            cell.SetCellValue("Payment Year");
            cell.AddComment("Shows all the payments that have been made in each monthly run.");

            cell = currentRow.CreateCell(++cellNumber);
            cell.CellStyle = _context.Styles[Style.Bold];
            cell.SetCellValue("PaymentPeriod");

            cell = currentRow.CreateCell(++cellNumber);
            cell.CellStyle = _context.Styles[Style.Bold];
            cell.SetCellValue("NumPayments");

            cell = currentRow.CreateCell(++cellNumber);
            cell.CellStyle = _context.Styles[Style.Bold];
            cell.SetCellValue("PaymentsAmount");
        }

        private void AddRows(IEnumerable<PaymentsMade> paymentsMade)
        {
            paymentsMade.OrderBy(p => p.Order).ToList().ForEach(p => AddRow(p));
        }

        private void AddRow(PaymentsMade paymentsMade)
        {
            var currentRow = _context.Sheet.GetOrCreateRow(_context.RowNumber++);
            var cellNumber = _context.StartCellNumber;

            var cell = currentRow.CreateCell(cellNumber);
            cell.CellStyle = _context.Styles[Style.Default];
            cell.SetCellValue(paymentsMade.Year);

            cell = currentRow.CreateCell(++cellNumber);
            cell.CellStyle = _context.Styles[Style.Default];
            cell.SetCellValue(paymentsMade.Period);

            cell = currentRow.CreateCell(++cellNumber);
            cell.CellStyle = _context.Styles[Style.Default];
            cell.SetCellValue(paymentsMade.Number);

            cell = currentRow.CreateCell(++cellNumber);
            cell.CellStyle = _context.Styles[Style.Default];
            cell.SetCellValue(paymentsMade.Amount);
        }

        private void AddTotalRow(IEnumerable<PaymentsMade> paymentsMade)
        {
            var currentPeriodRow = _context.Sheet.GetOrCreateRow(_context.RowNumber++);
            var cellNumber = _context.StartCellNumber + 1;

            var cell = currentPeriodRow.CreateCell(cellNumber);
            cell.CellStyle = _context.Styles[Style.Bold];
            cell.SetCellValue("Total");

            cell = currentPeriodRow.CreateCell(++cellNumber);
            cell.CellStyle = _context.Styles[Style.Bold];
            cell.SetCellValue(paymentsMade.Sum(p => p.Number));

            cell = currentPeriodRow.CreateCell(++cellNumber);
            cell.CellStyle = _context.Styles[Style.CurrencyBold];
            cell.SetCellValue(paymentsMade.Sum(p => p.Amount));
        }

        private void AddPercentagePaidRow(CollectionPeriod collectionPeriod, IEnumerable<PaymentsMade> paymentsMade, PeriodValidationSummary validationSummary)
        {
            var currentPeriodRow = _context.Sheet.GetOrCreateRow(_context.RowNumber++);
            var cellNumber = _context.StartCellNumber;

            var cell = currentPeriodRow.CreateCell(cellNumber);
            cell.SetCellValue("Percentage of validation records paid");            
            cellNumber += 3;
            

            cell = currentPeriodRow.CreateCell(cellNumber);
            cell.CellStyle = _context.Styles[Style.Percentage];

            if (validationSummary.ValidRecords.Count > 0)
            {
                cell.SetCellValue((paymentsMade.Where(p => p.Year == collectionPeriod.AcademicYear && p.Period == collectionPeriod.Period).Sum(p => p.Amount)) / validationSummary.ValidRecords.PeriodAmount);
            }
            cell.AddComment("If this is not 100% then there are some records which have passed validation which have not been paid. Check the Payments table against any 'unpaid' validation records.");
        }
    }
}
