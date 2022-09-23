using Microsoft.Extensions.Http;
using System;
using Polly;
using System.Net.Http;
using Polly.Extensions.Http;

namespace SFA.DAS.EmployerIncentives.Abstractions.API
{
    public sealed class TransientRetryHandler : PolicyHttpMessageHandler
    {
        public TransientRetryHandler(Func<PolicyBuilder<HttpResponseMessage>, IAsyncPolicy<HttpResponseMessage>> configurePolicy)
            : base(configurePolicy(HttpPolicyExtensions.HandleTransientHttpError()))
        {
        }
    }
}
