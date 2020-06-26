using System;
using System.Collections.Generic;
using System.Text;
using SFA.DAS.EmployerIncentives.Spike.Api.Client.Configuration;
using SFA.DAS.Http;

namespace SFA.DAS.EmployerIncentives.Spike.Api.Client
{
    public class SimpleApiClientFactory : ISimpleApiClientFactory
    {
        public SimpleApiClientFactory()
        {
        }
        public ISimpleApiClient CreateClient()
        {
            var httpClient = new HttpClientBuilder().WithDefaultHeaders().Build();
            httpClient.BaseAddress = new Uri("https://localhost:44359");


            var restHttpClient = new EmployerCommitmentsRestHttpClient(httpClient);
            var apiClient = new SimpleApiClient(restHttpClient);

            return apiClient;
        }
    }
}
