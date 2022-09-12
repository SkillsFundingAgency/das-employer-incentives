using System.Data.Common;

namespace SFA.DAS.EmployerIncentives.Data.Models
{
    public interface IEmployerIncentivesDbContextFactory
    {
        EmployerIncentivesDbContext Create(DbTransaction transaction = null);
    }
}
