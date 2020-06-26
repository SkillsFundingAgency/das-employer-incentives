using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace SFA.DAS.EmployerIncentives.Spike.Api.Client.Types
{
    [Serializable]
    public class ErrorResponse
    {
        [JsonProperty(Required = Required.Always)]
        public List<ErrorDetail> Errors { get; }

        [JsonConstructor]
        public ErrorResponse(List<ErrorDetail> errors)
        {
            Errors = errors;
        }
    }

}
