using NPOI.SS.UserModel;
using System.Collections.Generic;

namespace SFA.DAS.EmployerIncentives.Reports.Excel
{
    public class Context
    {
        public ISheet Sheet { get; private set; }
        public int RowNumber { get; set; }
        public int StartCellNumber { get; private set; }
        public IDictionary<Style, ICellStyle> Styles { get; private set; }

        public Context(ISheet sheet, IDictionary<Style, ICellStyle> styles, int rowNumber = 0, int cellNumber = 0)
        {
            Sheet = sheet;
            RowNumber = rowNumber;
            StartCellNumber = cellNumber;
            Styles = styles;
        }
    }
}
