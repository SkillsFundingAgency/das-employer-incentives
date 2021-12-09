using Newtonsoft.Json;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace SFA.DAS.EmployerIncentives.Commands.Services.LearnerMatchApi
{
    public class LearnerSubmissionService : ILearnerSubmissionService
    {
        private readonly HttpClient _client;
        private readonly string _serviceVersion;
        private readonly ILogger<LearnerService> _logger;

        public LearnerSubmissionService(
            HttpClient client,
            string serviceVersion,
            ILogger<LearnerService> logger)
        {
            _client = client;
            _serviceVersion = serviceVersion;
            _logger = logger;
        }

        public async Task<LearnerSubmissionDto> Get(Learner learner)
        {
            _logger.LogDebug("Start Learner data refresh from Learner match service for ApprenticeshipIncentiveId: {ApprenticeshipIncentiveId}, ApprenticeshipId: {ApprenticeshipId}, UKPRN: {UKPRN}, ULN: {ULN}",
                learner.ApprenticeshipIncentiveId, learner.ApprenticeshipId, learner.Ukprn, learner.UniqueLearnerNumber);
            
            var response = await _client.GetAsync($"api/v{_serviceVersion}/{learner.Ukprn}/{learner.UniqueLearnerNumber}?");

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }

            response.EnsureSuccessStatusCode();

            var jsonString = await response.Content.ReadAsStringAsync();
            var data = JsonConvert.DeserializeObject<LearnerSubmissionDto>(jsonString);
            data.RawJson = jsonString;
            
            _logger.LogDebug("End Learner data refresh from Learner match service for ApprenticeshipIncentiveId: {ApprenticeshipIncentiveId}, ApprenticeshipId: {ApprenticeshipId}, UKPRN: {UKPRN}, ULN: {ULN}",
                learner.ApprenticeshipIncentiveId, learner.ApprenticeshipId, learner.Ukprn, learner.UniqueLearnerNumber);

            return data;
        }
    }
}
