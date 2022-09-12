using System.Data.Common;
using Microsoft.EntityFrameworkCore;

namespace SFA.DAS.EmployerIncentives.Data.Models
{
    public class EmployerIncentivesDbContextFactory : IEmployerIncentivesDbContextFactory
    {
        private readonly DbContextOptions<EmployerIncentivesDbContext> _options;

        public EmployerIncentivesDbContextFactory(DbContextOptions<EmployerIncentivesDbContext> optionsBuilder)
        {
            _options = optionsBuilder;
        }

        public EmployerIncentivesDbContext Create(DbTransaction transaction = null)
        {
            var context = new EmployerIncentivesDbContext(_options);
            if (transaction != null)
                context.Database.UseTransaction(transaction);
            return context;
        }
    }
}
