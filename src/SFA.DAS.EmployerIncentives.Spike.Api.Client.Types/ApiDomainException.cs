using System;
using System.Collections.Generic;
using System.Text;

namespace SFA.DAS.EmployerIncentives.Spike.Api.Client.Types
{
    public class ApiDomainException : Exception
    {
        public List<ErrorDetail> Errors { get; }

        public ApiDomainException(List<ErrorDetail> errors) : base("Validation Exception")
        {
            Errors = errors;
        }
    }
}
