using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NServiceBus.ObjectBuilder.MSDependencyInjection;
using SFA.DAS.Configuration.AzureTableStorage;
using SFA.DAS.EmployerIncentives.Api.Extensions;
using SFA.DAS.EmployerIncentives.Commands;
using SFA.DAS.EmployerIncentives.Data.Models;
using SFA.DAS.EmployerIncentives.Infrastructure;
using SFA.DAS.EmployerIncentives.Infrastructure.Configuration;
using SFA.DAS.EmployerIncentives.Queries;
using SFA.DAS.UnitOfWork.EntityFrameworkCore.DependencyResolution.Microsoft;
using System;
using System.IO;

namespace SFA.DAS.EmployerIncentives.Api
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;

            var config = new ConfigurationBuilder()
                .AddConfiguration(configuration)
                .SetBasePath(Directory.GetCurrentDirectory());

            if (!configuration["Environment"].Equals("LOCAL", StringComparison.CurrentCultureIgnoreCase))
            {
                config.AddAzureTableStorage(options =>
                {
                    options.ConfigurationKeys = configuration["ConfigNames"].Split(",");
                    options.StorageConnectionString = configuration["ConfigurationStorageConnectionString"];
                    options.EnvironmentName = configuration["Environment"];
                    options.PreFixConfigurationKeys = false;
                });
            }
#if DEBUG
            config.AddJsonFile($"appsettings.Development.json", optional: true);
#endif
            Configuration = config.Build();
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddNLog();
            services.AddControllers();
            services.AddApplicationInsightsTelemetry();
            services.AddHealthChecks();
            services.AddSwaggerGen();

            services.AddOptions();
            services.Configure<ApplicationSettings>(Configuration.GetSection("ApplicationSettings"));
            services.Configure<PolicySettings>(Configuration.GetSection("PolicySettings"));
            services.Configure<AccountApi>(Configuration.GetSection("AccountApi"));
            services.AddCommandServices();
            services.AddQueryServices();

            services.AddDbContext<EmployerIncentivesDbContext>((options) =>
            {
                options.UseSqlServer(Configuration["ApplicationSettings:DbConnectionString"]);
            })
            .AddEntityFrameworkUnitOfWork<EmployerIncentivesDbContext>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();

                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("swagger/v1/swagger.json", "Employer Incentives API");
                    c.RoutePrefix = string.Empty;
                });
            }

            app.UseHttpsRedirection()
               .UseApiGlobalExceptionHandler();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHealthChecks("/health");
            });
        }

        public void ConfigureContainer(UpdateableServiceProvider serviceProvider)
        {
            serviceProvider.StartNServiceBus(Configuration).GetAwaiter().GetResult();
        }
    }
}
