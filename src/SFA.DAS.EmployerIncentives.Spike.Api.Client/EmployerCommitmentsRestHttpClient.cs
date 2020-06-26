using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SFA.DAS.EmployerIncentives.Spike.Api.Client.Types;
using SFA.DAS.Http;

namespace SFA.DAS.EmployerIncentives.Spike.Api.Client
{
    public class EmployerCommitmentsRestHttpClient : RestHttpClient
    {

        public EmployerCommitmentsRestHttpClient(HttpClient httpClient) : base(httpClient)
        {
        }

        protected override Exception CreateClientException(HttpResponseMessage httpResponseMessage, string content)
        {
            return httpResponseMessage.StatusCode == HttpStatusCode.BadRequest && httpResponseMessage.GetSubStatusCode() == HttpSubStatusCode.DomainException
                ? CreateApiModelException(httpResponseMessage, content)
                : base.CreateClientException(httpResponseMessage, content);
        }

        private Exception CreateApiModelException(HttpResponseMessage httpResponseMessage, string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return new ApiDomainException(new List<ErrorDetail>());
            }

            var errors = new ApiDomainException(JsonConvert.DeserializeObject<ErrorResponse>(content).Errors);

            return errors;
        }
    }
}
