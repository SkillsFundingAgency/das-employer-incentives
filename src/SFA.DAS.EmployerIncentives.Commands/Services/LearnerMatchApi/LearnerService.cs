using Newtonsoft.Json;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
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
        private readonly IApprenticeshipIncentiveDomainRepository _domainRepository;

        public LearnerService(
            HttpClient client,
            string serviceVersion,
            IApprenticeshipIncentiveDomainRepository domainRepository)
        {
            _client = client;
            _serviceVersion = serviceVersion;
            _domainRepository = domainRepository;
        }

        public async Task Refresh(Learner learner)
        {
            var incentive = await _domainRepository.Find(learner.ApprenticeshipIncentiveId);
            if(incentive == null)
            {
                throw new ArgumentException("Apprenticeship inventive does not exist for learner record");
            }

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

            var firstPendingPayment = incentive.PendingPayments.Where(p => p.PaymentMadeDate == null).OrderBy(p => p.DueDate).FirstOrDefault();
            submissionData.SetIsInLearning(learnerSubmissionDto.InLearningForAppenticeship(learner.ApprenticeshipId, firstPendingPayment));

            submissionData.SetRawJson(jsonString);
            learner.SetSubmissionData(submissionData);
        }
    }
}
