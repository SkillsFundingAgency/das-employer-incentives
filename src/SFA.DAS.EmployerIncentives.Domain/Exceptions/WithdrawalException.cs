using SFA.DAS.EmployerIncentives.Abstractions.Domain;
using System;
using System.Runtime.Serialization;

namespace SFA.DAS.EmployerIncentives.Domain.Exceptions
{
    [Serializable]
    public class WithdrawalException : DomainException
    {
        public WithdrawalException() { }
        public WithdrawalException(string message) : base(message) { }
        public WithdrawalException(string message, Exception innerException) : base(message, innerException) { }
        protected WithdrawalException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
