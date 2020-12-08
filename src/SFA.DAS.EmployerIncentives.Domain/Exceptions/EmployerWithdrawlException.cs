using SFA.DAS.EmployerIncentives.Abstractions.Domain;
using System;
using System.Runtime.Serialization;

namespace SFA.DAS.EmployerIncentives.Domain.Exceptions
{
    [Serializable]
    public class EmployerWithdrawlException : DomainException
    {
        public EmployerWithdrawlException() { }
        public EmployerWithdrawlException(string message) : base(message) { }
        public EmployerWithdrawlException(string message, Exception innerException) : base(message, innerException) { }
        protected EmployerWithdrawlException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
