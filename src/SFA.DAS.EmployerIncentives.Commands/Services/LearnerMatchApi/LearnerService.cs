using Newtonsoft.Json;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives;
using System.Net.Http;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Commands.Services.LearnerMatchApi
{
    public class LearnerService : ILearnerService
    {
        private readonly HttpClient _client;
        private readonly string _serviceVersion;

        public LearnerService(
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
            data.RawJson = jsonString;

            return data;
        }
    }
}
