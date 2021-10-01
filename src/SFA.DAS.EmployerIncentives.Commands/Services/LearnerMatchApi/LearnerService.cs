using System;
using Newtonsoft.Json;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace SFA.DAS.EmployerIncentives.Commands.Services.LearnerMatchApi
{
    public class LearnerService : ILearnerService
    {
        private readonly HttpClient _client;
        private readonly string _serviceVersion;
        private readonly ILogger<LearnerService> _logger;

        public LearnerService(
            HttpClient client,
            string serviceVersion,
            ILogger<LearnerService> logger)
        {
            _client = client;
            _serviceVersion = serviceVersion;
            _logger = logger;
        }

        public async Task<LearnerServiceResponse> Get(Learner learner)
        {
            var response = await _client.GetAsync($"api/v{_serviceVersion}/{learner.Ukprn}/{learner.UniqueLearnerNumber}?");

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }

            response.EnsureSuccessStatusCode();

            var learnerServiceResponse = new LearnerServiceResponse
            {
                RawJson = await response.Content.ReadAsStringAsync()
            };

            try
            {
                learnerServiceResponse.LearnerSubmissionDto = JsonConvert.DeserializeObject<LearnerSubmissionDto>(learnerServiceResponse.RawJson);
            }
            catch (Exception)
            {
                _logger.LogError($"Unable to deserialize response from Learner match API - JSON: {learnerServiceResponse.RawJson}");
            }

            return learnerServiceResponse;
        }
    }
}
