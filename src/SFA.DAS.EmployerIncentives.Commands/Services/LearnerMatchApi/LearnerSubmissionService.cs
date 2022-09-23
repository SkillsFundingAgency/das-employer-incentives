using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives;
using SFA.DAS.EmployerIncentives.Infrastructure.Configuration;
using System.Net.Http;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Commands.Services.LearnerMatchApi
{
    public class LearnerSubmissionService : ILearnerSubmissionService
    {
        private readonly HttpClient _client;
        private readonly string _serviceVersion;

        public LearnerSubmissionService(
            HttpClient client,
            IOptions<MatchedLearnerApi> config)
        {            
            _client = client;
            _serviceVersion = config.Value.Version;
        }

        public async Task<LearnerSubmissionDto> Get(Learner learner)
        {
            var response = await _client.GetAsync($"api/v{_serviceVersion}/{learner.Ukprn}/{learner.UniqueLearnerNumber}?");

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }

            response.EnsureSuccessStatusCode();

            var jsonString = await response.Content.ReadAsStringAsync();
            var data = JsonConvert.DeserializeObject<LearnerSubmissionDto>(jsonString);

            if (data.Uln != learner.UniqueLearnerNumber)
            {
                throw new InvalidLearnerMatchResponseException(jsonString);
            }

            data.RawJson = jsonString;

            return data;
        }
    }
}
