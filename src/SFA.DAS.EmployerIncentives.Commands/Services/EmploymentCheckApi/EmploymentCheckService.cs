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
            var createEmploymentCheckRequest = new CreateEmploymentCheckRequestDto(correlationId, employmentCheck.CheckType.ToString(), apprenticeshipIncentive.Apprenticeship.UniqueLearnerNumber, apprenticeshipIncentive.Account.Id, apprenticeshipIncentive.Apprenticeship.Id, employmentCheck.MinimumDate, employmentCheck.MaximumDate);
            var content = new StringContent(JsonConvert.SerializeObject(createEmploymentCheckRequest), Encoding.Default, "application/json");
            var response = await _client.PutAsync("employmentchecks", content);

            if (response.StatusCode == HttpStatusCode.Accepted)
            {
                return correlationId;
            }

            throw new EmploymentCheckApiException(response.StatusCode);
        }
    }
}
