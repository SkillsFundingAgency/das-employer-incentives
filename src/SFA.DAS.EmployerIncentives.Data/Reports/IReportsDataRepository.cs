using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Data.Reports
{
    public interface IReportsDataRepository
    {
        Task<T> Execute<T>() where T : class, IReport;
    }
}
