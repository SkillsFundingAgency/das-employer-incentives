using SFA.DAS.EmployerIncentives.Abstractions.Domain;
using System;
using System.Runtime.Serialization;

namespace SFA.DAS.EmployerIncentives.Domain.Exceptions
{
    [Serializable]
    public class EmployerWithdrawalException : DomainException
    {
        public EmployerWithdrawalException() { }
        public EmployerWithdrawalException(string message) : base(message) { }
        public EmployerWithdrawalException(string message, Exception innerException) : base(message, innerException) { }
        protected EmployerWithdrawalException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
