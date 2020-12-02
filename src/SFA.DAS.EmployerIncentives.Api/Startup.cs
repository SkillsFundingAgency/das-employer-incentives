using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using NServiceBus.ObjectBuilder.MSDependencyInjection;
using SFA.DAS.Configuration.AzureTableStorage;
using SFA.DAS.EmployerIncentives.Api.Extensions;
using SFA.DAS.EmployerIncentives.Commands;
using SFA.DAS.EmployerIncentives.Data.Models;
using SFA.DAS.EmployerIncentives.Events;
using SFA.DAS.EmployerIncentives.Infrastructure.Configuration;
using SFA.DAS.EmployerIncentives.Queries;
using SFA.DAS.UnitOfWork.EntityFrameworkCore.DependencyResolution.Microsoft;
using SFA.DAS.UnitOfWork.NServiceBus.Features.ClientOutbox.DependencyResolution.Microsoft;
using System;
using System.Data.Common;
using System.Data.SqlClient;
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
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddEnvironmentVariables();

            if (!ConfigurationIsAcceptanceTests())
            {
                config.AddAzureTableStorage(options =>
                {
                    options.ConfigurationKeys = configuration["ConfigNames"].Split(",");
                    options.StorageConnectionString = configuration["ConfigurationStorageConnectionString"];
                    options.EnvironmentName = configuration["EnvironmentName"];
                    options.PreFixConfigurationKeys = false;
                });
            }
#if DEBUG
            if (!configuration["EnvironmentName"].Equals("LOCAL_ACCEPTANCE_TESTS", StringComparison.CurrentCultureIgnoreCase))
            {
                config.AddJsonFile($"appsettings.Development.json", optional: true);
            }
#endif
            Configuration = config.Build();
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddApplicationInsightsTelemetry();
            services.AddHealthChecks();
            services.AddSwaggerGen();

            services.Configure<AzureActiveDirectoryConfiguration>(Configuration.GetSection("AzureAd"));
            services.AddSingleton(cfg => cfg.GetService<IOptions<AzureActiveDirectoryConfiguration>>().Value);

            if (!ConfigurationIsLocalOrDevOrAcceptanceTests())
            {
                var azureAdConfiguration = Configuration
                    .GetSection("AzureAd")
                    .Get<AzureActiveDirectoryConfiguration>();

                services.AddAuthentication(azureAdConfiguration);
            }

            services.AddOptions();
            services.Configure<ApplicationSettings>(Configuration.GetSection("ApplicationSettings"));
            services.Configure<PolicySettings>(Configuration.GetSection("PolicySettings"));
            services.Configure<AccountApi>(Configuration.GetSection("AccountApi"));
            services.Configure<MatchedLearnerApi>(Configuration.GetSection("MatchedLearnerApi"));
            services.Configure<EmailTemplateSettings>(Configuration.GetSection("EmailTemplates"));
            
            services.AddEntityFrameworkForEmployerIncentives()
                .AddEntityFrameworkUnitOfWork<EmployerIncentivesDbContext>()
                .AddNServiceBusClientUnitOfWork();

            services.AddPersistenceServices();
            services.AddCommandServices();
            services.AddQueryServices();
            services.AddEventServices();

            services
                .AddMvc(o =>
                {
                    if (!ConfigurationIsLocalOrDevOrAcceptanceTests())
                    {
                        o.Conventions.Add(new AuthorizeControllerModelConvention());
                    }
                }).SetCompatibilityVersion(CompatibilityVersion.Version_3_0);

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

            app.UseAuthentication();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHealthChecks("/ping");
            });
        }

        public void ConfigureContainer(UpdateableServiceProvider serviceProvider)
        {
            serviceProvider.StartNServiceBus(Configuration).GetAwaiter().GetResult();
        }

        private bool ConfigurationIsAcceptanceTests()
        {
            return Configuration["EnvironmentName"].Equals("LOCAL_ACCEPTANCE_TESTS", StringComparison.CurrentCultureIgnoreCase);
        }

        private bool ConfigurationIsLocalOrDevOrAcceptanceTests()
        {
            return Configuration["EnvironmentName"].Equals("LOCAL", StringComparison.CurrentCultureIgnoreCase) ||
                   Configuration["EnvironmentName"].Equals("DEV", StringComparison.CurrentCultureIgnoreCase) ||
                   Configuration["EnvironmentName"].Equals("LOCAL_ACCEPTANCE_TESTS", StringComparison.CurrentCultureIgnoreCase);
        }
    }
}
