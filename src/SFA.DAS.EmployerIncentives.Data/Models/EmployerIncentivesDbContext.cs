using Microsoft.AspNetCore.Hosting;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;

namespace SFA.DAS.EmployerIncentives.Data.Models
{
    public partial class EmployerIncentivesDbContext : DbContext
    {
        private readonly AzureServiceTokenProvider _azureServiceTokenProvider;
        private readonly IConfiguration _configuration;
        private readonly IHostingEnvironment _hostingEnvironment;

        public EmployerIncentivesDbContext()
        {
        }

        public EmployerIncentivesDbContext(DbContextOptions<EmployerIncentivesDbContext> options)
            : base(options)
        {
        }

        public EmployerIncentivesDbContext(DbContextOptions<EmployerIncentivesDbContext> options, AzureServiceTokenProvider azureServiceTokenProvider, IConfiguration configuration, IHostingEnvironment hostingEnvironment)
            : base(options)
        {
            _azureServiceTokenProvider = azureServiceTokenProvider;
            _configuration = configuration;
            _hostingEnvironment = hostingEnvironment;
        }

        public virtual DbSet<Account> Accounts { get; set; }
        public virtual DbSet<IncentiveApplication> Applications { get; set; }
        public virtual DbSet<IncentiveApplicationApprenticeship> ApplicationApprenticeships { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder dbContextOptionsBuilder)
        {
            if (!dbContextOptionsBuilder.IsConfigured)
            {
                SqlConnection connection = new SqlConnection();
                connection.ConnectionString = _configuration["ApplicationSettings:DbConnectionString"];
                if (!_hostingEnvironment.IsDevelopment() && !_hostingEnvironment.EnvironmentName.Contains("LOCAL", StringComparison.CurrentCultureIgnoreCase))
                    connection.AccessToken = _azureServiceTokenProvider.GetAccessTokenAsync("https://database.windows.net/").GetAwaiter().GetResult();

                dbContextOptionsBuilder.UseSqlServer(connection);
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

            modelBuilder.Entity<IncentiveApplicationApprenticeship>().Property(x => x.ApprenticeshipEmployerTypeOnApproval).HasConversion<int>();

            //modelBuilder.Entity<IncentiveApplication>(entity => entity.HasMany<IncentiveApplicationApprenticeship>());

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
