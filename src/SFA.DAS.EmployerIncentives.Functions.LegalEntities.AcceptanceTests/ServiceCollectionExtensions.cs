using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SFA.DAS.EmployerIncentives.Data.Models;

namespace SFA.DAS.EmployerIncentives.Functions.LegalEntities.AcceptanceTests
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection UseTestDb(this IServiceCollection serviceCollection, TestContext context)
        {
            var optionsBuilder = new DbContextOptionsBuilder<EmployerIncentivesDbContext>();
            optionsBuilder.UseSqlServer(context.SqlDatabase.DatabaseInfo.ConnectionString);
            serviceCollection.AddScoped(c => optionsBuilder.Options);

            return serviceCollection;
        }
    }
}
