using Microsoft.EntityFrameworkCore;
using SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives.Models;

namespace SFA.DAS.EmployerIncentives.Data.Models
{
    public partial class EmployerIncentivesDbContext : DbContext
    {
        public EmployerIncentivesDbContext()
        {
        }

        public EmployerIncentivesDbContext(DbContextOptions<EmployerIncentivesDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Account> Accounts { get; set; }
        public virtual DbSet<IncentiveApplication> Applications { get; set; }
        public virtual DbSet<IncentiveApplicationApprenticeship> ApplicationApprenticeships { get; set; }
        public virtual DbSet<ApprenticeshipIncentive> ApprenticeshipIncentives { get; set; }
        public virtual DbSet<PendingPayment> PendingPayments { get; set; }
        public virtual DbSet<Payment> Payments { get; set; }
        public virtual DbSet<CollectionPeriod> CollectionPeriods { get; set; }
        public virtual DbSet<PendingPaymentValidationResult> PendingPaymentValidationResults { get; set; }
        public virtual DbSet<Learner> Learners { get; set; }
        public virtual DbSet<LearningPeriod> LearningPeriods { get; set; }
        public virtual DbSet<ApprenticeshipDaysInLearning> DaysInLearnings { get; set; }
        public virtual DbSet<IncentiveApplicationStatusAudit> IncentiveApplicationStatusAudits { get; set; }
        public virtual DbSet<ClawbackPayment> ClawbackPayments { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Account>(entity =>
            {
                entity.HasKey(e => new { e.Id, e.AccountLegalEntityId })
                    .IsClustered(false);

                entity.Property(e => e.LegalEntityName)
                    .IsRequired()
                    .IsUnicode(false);
            });

            modelBuilder.Entity<IncentiveApplicationApprenticeship>().Property(x => x.ApprenticeshipEmployerTypeOnApproval).HasConversion<int>();
            modelBuilder.Entity<ApprenticeshipIncentive>().Property(x => x.EmployerType).HasConversion<int>();
            modelBuilder.Entity<Payment>().Property(x => x.SubnominalCode).HasConversion<int>();

            modelBuilder.Entity<LearningPeriod>(entity =>
            {
                entity.HasKey(e => new { e.LearnerId, e.StartDate });
            });

            modelBuilder.Entity<ApprenticeshipDaysInLearning>(entity =>
            {
                entity.HasKey(e => new { e.LearnerId, e.CollectionPeriodYear, e.CollectionPeriodNumber });
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
