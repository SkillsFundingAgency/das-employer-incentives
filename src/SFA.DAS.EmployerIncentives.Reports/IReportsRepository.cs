using System.IO;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Reports
{
    public interface IReportsRepository
    {
        Task Save(ReportsFileInfo fileInfo, Stream stream);
        Task<byte[]> Get(ReportsFileInfo fileInfo);
    }
}
