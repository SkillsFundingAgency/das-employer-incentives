using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives;

namespace SFA.DAS.EmployerIncentives.Commands.Services.EmploymentCheckApi
{
    public class EmploymentCheckService : IEmploymentCheckService
    {
        private readonly HttpClient _client;

        public EmploymentCheckService(HttpClient client)
        {
            _client = client;
        }

        public async Task<Guid> RegisterEmploymentCheck(EmploymentCheck employmentCheck, Domain.ApprenticeshipIncentives.ApprenticeshipIncentive apprenticeshipIncentive)
        {
            var correlationId = Guid.NewGuid();
            var createEmploymentCheckRequest = new RegisterCheckRequest
            {
                ApprenticeshipAccountId = apprenticeshipIncentive.Account.Id,
                ApprenticeshipId = apprenticeshipIncentive.Apprenticeship.Id,
                CheckType = employmentCheck.CheckType.ToString(),
                CorrelationId = correlationId,
                MaxDate = employmentCheck.MaximumDate,
                MinDate = employmentCheck.MinimumDate,
                Uln = apprenticeshipIncentive.Apprenticeship.UniqueLearnerNumber
            };
            var content = new StringContent(JsonConvert.SerializeObject(createEmploymentCheckRequest), Encoding.Default, "application/json");
            var response = await _client.PostAsync("employmentchecks", content);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                return correlationId;
            }

            throw new EmploymentCheckApiException(response.StatusCode);
        }
    }
}
