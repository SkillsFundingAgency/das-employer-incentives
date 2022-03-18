using SFA.DAS.EmployerIncentives.Data.Reports;
using System;
using System.IO;

namespace SFA.DAS.EmployerIncentives.Reports.Excel
{
    public interface IExcelReportGenerator<in T> : IDisposable where T : class, IReport
    {
        Stream Create(T report);
    }
}
