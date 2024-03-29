﻿using SFA.DAS.EmployerIncentives.Data.Reports.Metrics;
using System.Collections.Generic;
using System.Linq;

namespace SFA.DAS.EmployerIncentives.Reports.Excel.Metrics
{
    public class AccountedForEarningsTable
    {
        private readonly Context _context;

        public AccountedForEarningsTable(Context context)
        {
            _context = context;
        }

        public void Create(MetricsReport report)
        {
            AddHeaderRow(report.CollectionPeriod, report.PaymentsMade, report.ValidationSummary);
            AddDifferenceRow(report.CollectionPeriod, report.PaymentsMade, report.ValidationSummary);
            _context.RowNumber++;
        }

        private void AddHeaderRow(CollectionPeriod collectionPeriod, IEnumerable<PaymentsMade> paymentsMade, PeriodValidationSummary validationSummary)
        {
            var currentRow = _context.Sheet.GetOrCreateRow(_context.RowNumber++);
            var cellNumber = _context.StartCellNumber;

            var cell = currentRow.CreateCell(cellNumber);
            cell.CellStyle = _context.Styles[Style.Bold];
            cell.SetCellValue("Accounted for earnings");
            cell.AddComment("Paid vs Validation fails");

            cellNumber += 3;

            cell = currentRow.CreateCell(cellNumber++);
            cell.CellStyle = _context.Styles[Style.CurrencyBold];
            cell.SetCellValue(paymentsMade.Where(p => p.Year == collectionPeriod.AcademicYear).Sum(p => p.Amount) + validationSummary.InvalidRecords.PeriodAmount);

            cell = currentRow.CreateCell(cellNumber);
            cell.CellStyle = _context.Styles[Style.CurrencyBold];
            cell.SetCellValue(validationSummary.ValidRecords.YtdAmount + validationSummary.InvalidRecords.YtdAmount);
        }

        private void AddDifferenceRow(CollectionPeriod collectionPeriod, IEnumerable<PaymentsMade> paymentsMade, PeriodValidationSummary validationSummary)
        {
            var currentRow = _context.Sheet.GetOrCreateRow(_context.RowNumber++);
            var cellNumber = _context.StartCellNumber;

            var cell = currentRow.CreateCell(cellNumber);
            cell.CellStyle = _context.Styles[Style.Bold];
            cell.SetCellValue("Difference");

            cellNumber += 3;

            cell = currentRow.CreateCell(cellNumber++);
            cell.CellStyle = _context.Styles[Style.Percentage];
            cell.AddComment("A non-zero percentage means that either we have accounted for some earnings more than once " +
                "or not accounted for some earnings");

            if ((validationSummary.ValidRecords.YtdAmount + validationSummary.InvalidRecords.YtdAmount) > 0)
            {
                cell.SetCellValue((paymentsMade.Where(p => p.Year == collectionPeriod.AcademicYear).Sum(p => p.Amount) + validationSummary.InvalidRecords.PeriodAmount) / (validationSummary.ValidRecords.YtdAmount + validationSummary.InvalidRecords.YtdAmount));
            }

            cell = currentRow.CreateCell(cellNumber);
            cell.CellStyle = _context.Styles[Style.CurrencyBold];
            cell.SetCellValue((validationSummary.ValidRecords.YtdAmount + validationSummary.InvalidRecords.YtdAmount) - (paymentsMade.Where(p => p.Year == collectionPeriod.AcademicYear).Sum(p => p.Amount) + validationSummary.InvalidRecords.PeriodAmount));
        }
    }
}
