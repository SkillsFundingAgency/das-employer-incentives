using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace SFA.DAS.EmployerIncentives.Commands.Exceptions
{
    [Serializable]
    public class InvalidRequestException : Exception
    {        
        public InvalidRequestException() { }

        public InvalidRequestException(Dictionary<string, string> errorMessages): base(BuildErrorMessage(errorMessages))
        {
        }

        protected InvalidRequestException(SerializationInfo info, StreamingContext context) : base(info, context) { }

        private static string BuildErrorMessage(Dictionary<string, string> errorMessages)
        {
            if (!errorMessages.Any())
            {
                return "Request is invalid";
            }

            return "Request is invalid:\n" + errorMessages.Select(kvp => $"{kvp.Key}: {kvp.Value}").Aggregate((x, y) => $"{x}\n{y}");
        }
    }
}
