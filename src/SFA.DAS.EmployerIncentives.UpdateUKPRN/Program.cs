using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace SFA.DAS.EmployerIncentives.UpdateUKPRN
{
    class Program
    {
        static void Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                                    .AddJsonFile("appsettings.json")
                                    .Build();

            Console.WriteLine("Fetching list of apprenticeships with no UKPRNs...");

            var employerIncentivesConnectionString = configuration["EmployerIncentivesConnectionString"];
            var employerIncentivesRepository = new EmployerIncentiveRepository(employerIncentivesConnectionString);
            var apprenticeshipIds = employerIncentivesRepository.GetApprenticeshipsWithoutUKPRN().GetAwaiter().GetResult();
            
            Console.WriteLine("Fetching UKPRNs for apprenticeship commitments...");

            var commitmentsConnectionString = configuration["CommitmentsConnectionString"];
            var commitmentsRepository = new CommitmentsRepository(commitmentsConnectionString);
            var apprenticeshipUKPRNs = commitmentsRepository.GetUKPRNsForApprenticeships(apprenticeshipIds).GetAwaiter().GetResult();

            var outputFileName = configuration["OutputFile"];

            Console.WriteLine($"Generating UKPRN update script to '{outputFileName}'...");

            var updateSqlScript = employerIncentivesRepository.GenerateUKPRNUpdateScript(apprenticeshipUKPRNs);

            File.WriteAllText(outputFileName, updateSqlScript);
        }
    }
}
