using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace SFA.DAS.EmployerIncentives.Spike.Api.Client.Types
{
    [Serializable]
    public class ErrorDetail
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Field { get; }

        [JsonProperty(Required = Required.Always)]
        public string Message { get; }

        [JsonConstructor]
        public ErrorDetail(string field, string message)
        {
            Field = string.IsNullOrWhiteSpace(field) ? null : field;
            Message = message;
        }
    }

}
