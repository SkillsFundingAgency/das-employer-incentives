﻿using SFA.DAS.EmployerIncentives.Data.Reports.Metrics;
using System.Collections.Generic;
using System.Linq;

namespace SFA.DAS.EmployerIncentives.Reports.Excel.Metrics
{
    public class EarningsSummaryTable
    {
        private readonly Context _context;

        public EarningsSummaryTable(Context context)
        {
            _context = context;
        }

        public void Create(MetricsReport report)
        {
            AddHeaderRow();
            AddYtdRow(report.CollectionPeriod, report.Earnings);
            AddClawbackRow(report.Clawbacks);
            AddSubTotalRow(report.CollectionPeriod, report.Earnings, report.Clawbacks);
            _context.RowNumber++;
        }

        private void AddHeaderRow()
        {
            var currentRow = _context.Sheet.GetOrCreateRow(_context.RowNumber++);
            var cellNumber = _context.StartCellNumber;

            var cell = currentRow.CreateCell(cellNumber);
            cell.CellStyle = _context.Styles[Style.Bold];
            cell.SetCellValue("Earnings");

            cellNumber += 4;

            cell = currentRow.CreateCell(cellNumber);
            cell.CellStyle = _context.Styles[Style.Bold];
            cell.SetCellValue("Amount");
        }

        private void AddYtdRow(CollectionPeriod collectionPeriod, IEnumerable<Earning> earnings)
        {
            var currentRow = _context.Sheet.GetOrCreateRow(_context.RowNumber++);
            var cellNumber = _context.StartCellNumber;

            var cell = currentRow.CreateCell(cellNumber);
            cell.CellStyle = _context.Styles[Style.Default];
            cell.SetCellValue($"R{collectionPeriod.Period} YTD Earnings");
            cell.AddComment("This shows the earnings which are 'due'. We use the YTD figure as we do not know when the earning will pass validation.  This means that it can be paid in any month on or after the date it is due.");

            cellNumber += 4;

            cell = currentRow.CreateCell(cellNumber);
            cell.CellStyle = _context.Styles[Style.Currency];
            cell.SetCellValue(earnings.Where(e => e.Year == collectionPeriod.AcademicYear && e.Period <= collectionPeriod.Period).Sum(e => e.Amount));
        }

        private void AddClawbackRow(Clawbacks clawbacks)
        {
            var currentRow = _context.Sheet.GetOrCreateRow(_context.RowNumber++);
            var cellNumber = _context.StartCellNumber;

            var cell = currentRow.CreateCell(cellNumber);
            cell.CellStyle = _context.Styles[Style.Default];
            cell.SetCellValue("Clawbacks");

            cellNumber += 4;

            cell = currentRow.CreateCell(cellNumber);
            cell.CellStyle = _context.Styles[Style.Currency];
            cell.SetCellValue(clawbacks.Sent + clawbacks.Unsent);
        }

        private void AddSubTotalRow(CollectionPeriod collectionPeriod, IEnumerable<Earning> earnings, Clawbacks clawbacks)
        {
            var currentRow = _context.Sheet.GetOrCreateRow(_context.RowNumber++);
            var cellNumber = _context.StartCellNumber;

            var cell = currentRow.CreateCell(cellNumber);
            cell.CellStyle = _context.Styles[Style.Bold];
            cell.SetCellValue("Earnings Sub-total");

            cellNumber += 4;

            cell = currentRow.CreateCell(cellNumber);
            cell.CellStyle = _context.Styles[Style.CurrencyBold];
            cell.SetCellValue(earnings.Where(e => e.Year == collectionPeriod.AcademicYear && e.Period <= collectionPeriod.Period).Sum(e => e.Amount) + (clawbacks.Sent + clawbacks.Unsent));
        }
    }
}
