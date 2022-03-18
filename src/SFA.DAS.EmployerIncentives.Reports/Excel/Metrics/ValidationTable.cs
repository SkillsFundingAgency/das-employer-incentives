using SFA.DAS.EmployerIncentives.Data.Reports.Metrics;
using System.Collections.Generic;
using System.Linq;

namespace SFA.DAS.EmployerIncentives.Reports.Excel.Metrics
{
    public class ValidationTable
    {
        private readonly Context _context;
        public ValidationTable(Context context)
        {
            _context = context;
        }

        public void Create(MetricsReport report, bool isYtd = false)
        {
            AddSectionHeaderRow(isYtd);
            AddHeaderRow();
            AddRows(isYtd ? report.YtdValidations : report.PeriodValidations);
            _context.RowNumber++;
        }

        private void AddSectionHeaderRow(bool isYtd)
        {
            var currentRow = _context.Sheet.GetOrCreateRow(_context.RowNumber++);
            var cellNumber = _context.StartCellNumber;

            var cell = currentRow.CreateCell(cellNumber);
            cell.CellStyle = _context.Styles[Style.Bold];
            cell.SetCellValue(isYtd ? "YTD validation" : "This period validation");
        }

        private void AddHeaderRow()
        {
            var currentRow = _context.Sheet.GetOrCreateRow(_context.RowNumber++);
            var cellNumber = _context.StartCellNumber;

            var cell = currentRow.CreateCell(cellNumber);
            cell.SetCellValue("CountOfPayments");

            cell = currentRow.CreateCell(++cellNumber);
            cell.SetCellValue("HasLearningRecord");

            cell = currentRow.CreateCell(++cellNumber);
            cell.SetCellValue("IsInLearning");

            cell = currentRow.CreateCell(++cellNumber);
            cell.SetCellValue("HasDaysInLearning");

            cell = currentRow.CreateCell(++cellNumber);
            cell.SetCellValue("HasNoDataLocks");

            cell = currentRow.CreateCell(++cellNumber);
            cell.SetCellValue("HasBankDetails");

            cell = currentRow.CreateCell(++cellNumber);
            cell.SetCellValue("PaymentsNotPaused");

            cell = currentRow.CreateCell(++cellNumber);
            cell.SetCellValue("HasNoUnsentClawbacks");

            cell = currentRow.CreateCell(++cellNumber);
            cell.SetCellValue("HasIlrSubmission");

            cell = currentRow.CreateCell(++cellNumber);
            cell.SetCellValue("HasSignedMinVersion");

            cell = currentRow.CreateCell(++cellNumber);
            cell.SetCellValue("Num Legal Entity IDs");

            cell = currentRow.CreateCell(++cellNumber);
            cell.SetCellValue("EarningAmount");

            _context.Sheet.SetAutoFilter(new NPOI.SS.Util.CellRangeAddress(_context.RowNumber - 1, _context.RowNumber - 1, _context.StartCellNumber, cellNumber));
        }

        private void AddRows(IEnumerable<Validation> ytdValidations)
        {
            ytdValidations.OrderBy(p => p.Order).ToList().ForEach(p => AddRow(p));
        }

        private void AddRow(Validation ytdValidation)
        {
            var currentRow = _context.Sheet.GetOrCreateRow(_context.RowNumber++);
            var cellNumber = _context.StartCellNumber;

            var cell = currentRow.CreateCell(cellNumber);
            cell.SetCellValue(ytdValidation.CountOfPayments);

            cell = currentRow.CreateCell(++cellNumber);
            cell.SetCellValue(ytdValidation.HasLearningRecord.ToInt());

            cell = currentRow.CreateCell(++cellNumber);
            cell.SetCellValue(ytdValidation.IsInLearning.ToInt());

            cell = currentRow.CreateCell(++cellNumber);
            cell.SetCellValue(ytdValidation.HasDaysInLearning.ToInt());

            cell = currentRow.CreateCell(++cellNumber);
            cell.SetCellValue(ytdValidation.HasNoDataLocks.ToInt());

            cell = currentRow.CreateCell(++cellNumber);
            cell.SetCellValue(ytdValidation.HasBankDetails.ToInt());

            cell = currentRow.CreateCell(++cellNumber);
            cell.SetCellValue(ytdValidation.PaymentsNotPaused.ToInt());

            cell = currentRow.CreateCell(++cellNumber);
            cell.SetCellValue(ytdValidation.HasNoUnsentClawbacks.ToInt());

            cell = currentRow.CreateCell(++cellNumber);
            cell.SetCellValue(ytdValidation.HasIlrSubmission.ToInt());

            cell = currentRow.CreateCell(++cellNumber);
            cell.SetCellValue(ytdValidation.HasSignedMinVersion.ToInt());

            cell = currentRow.CreateCell(++cellNumber);
            cell.SetCellValue(ytdValidation.NumberOfAccountLegalEntityIds);

            cell = currentRow.CreateCell(++cellNumber);
            cell.SetCellValue(ytdValidation.EarningAmount);
        }
    }
}
