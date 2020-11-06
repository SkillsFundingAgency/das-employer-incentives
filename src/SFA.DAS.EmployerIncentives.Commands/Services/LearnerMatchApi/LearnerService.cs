using Newtonsoft.Json;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;
using System;
using System.Linq;
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

        public async Task Refresh(Learner learner)
        {
            var response = await _client.GetAsync($"api/v{_serviceVersion}/{learner.Ukprn}/{learner.UniqueLearnerNumber}?");

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                learner.SetSubmissionData(null);

                return;
            }

            response.EnsureSuccessStatusCode();

            var jsonString = await response.Content.ReadAsStringAsync();
            var learnerSubmissionDto = JsonConvert.DeserializeObject<LearnerSubmissionDto>(jsonString);

            var submissionData = new SubmissionData(
                learnerSubmissionDto.IlrSubmissionDate,
                learnerSubmissionDto.Training.Any(t => t.Reference == "ZPROG001")
                );

            submissionData.SetStartDate(learnerSubmissionDto.LearningStartDateForAppenticeship(learner.ApprenticeshipId));

            submissionData.SetRawJson(jsonString);
            learner.SetSubmissionData(submissionData);
        }        
    }
}
