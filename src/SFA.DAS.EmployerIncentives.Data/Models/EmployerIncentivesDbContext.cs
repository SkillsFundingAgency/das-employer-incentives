﻿using Microsoft.EntityFrameworkCore;

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

            //modelBuilder.Entity<IncentiveApplication>(entity => entity.HasMany<IncentiveApplicationApprenticeship>());

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
