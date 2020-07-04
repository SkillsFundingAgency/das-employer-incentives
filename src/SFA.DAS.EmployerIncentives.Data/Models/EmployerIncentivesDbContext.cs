using Microsoft.EntityFrameworkCore;

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

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {                
                optionsBuilder.UseSqlServer("Server=.;Database=SFA.DAS.EmployerIncentives.Database;Trusted_Connection=True;");
            }
        }

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

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
