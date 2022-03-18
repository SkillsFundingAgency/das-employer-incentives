using System.Data;

namespace SFA.DAS.EmployerIncentives.Data.Reports
{
    public interface IReportsConnectionProvider
    {
        IDbConnection New();
    }
}
