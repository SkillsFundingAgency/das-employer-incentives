using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace SFA.DAS.EmployerIncentives.Reports.Reports
{
    public static class SheetExtensions
    {
        public static IRow GetOrCreateRow(this ISheet sheet, int rowNumber)
        {
            return sheet.GetRow(rowNumber) ?? sheet.CreateRow(rowNumber);
        }

        public static IRow AddBlankLineRow(this ISheet sheet, int rowNumber)
        {
            return sheet.CreateRow(rowNumber);
        }

        public static void SetColumnWidths(this ISheet sheet, int startColumn, int endColumn, int width)
        {
            for (int i = startColumn; i <= endColumn; i++)
            {
                sheet.SetColumnWidth(i, 255 * width);
            }
        }

        public static void AddComment(this ICell cell, string comment)
        {
            cell.CellComment = cell.Sheet.CreateDrawingPatriarch().CreateCellComment(cell.Sheet.Workbook.GetCreationHelper().CreateClientAnchor());
            cell.CellComment.String = new XSSFRichTextString(comment);
        }

        public static int ToInt(this bool value)
        {
            return value ? 1 : 0;
        }
    }
}
