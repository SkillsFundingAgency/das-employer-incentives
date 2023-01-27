using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;

namespace SFA.DAS.EmployerIncentives.Data.Account
{
    public interface IVendorBlockAuditRepository
    {
        Task Add(VendorBlockRequestAudit vendorBlockRequestAudit);
    }
}
