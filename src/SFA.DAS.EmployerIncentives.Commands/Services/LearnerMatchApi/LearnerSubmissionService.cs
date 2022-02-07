using Newtonsoft.Json;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives;
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
            string serviceVersion)
        {
            _client = client;
            _serviceVersion = serviceVersion;
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
