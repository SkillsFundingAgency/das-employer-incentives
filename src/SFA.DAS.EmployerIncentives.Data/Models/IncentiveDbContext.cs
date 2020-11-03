using Microsoft.EntityFrameworkCore;
using SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives.Models;

namespace SFA.DAS.EmployerIncentives.Data.Models
{
    public partial class IncentiveDbContext : DbContext
    {
        public IncentiveDbContext()
        {
        }

        public IncentiveDbContext(DbContextOptions<IncentiveDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<ApprenticeshipIncentive> ApprenticeshipIncentives { get; set; }
        public virtual DbSet<PendingPayment> PendingPayments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ApprenticeshipIncentive>().Property(x => x.EmployerType).HasConversion<int>();
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
