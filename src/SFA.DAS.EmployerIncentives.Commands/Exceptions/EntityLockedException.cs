using System;
using System.Runtime.Serialization;

namespace SFA.DAS.EmployerIncentives.Commands.Exceptions
{
    [Serializable]
    public sealed class EntityLockedException : Exception
    {
        public EntityLockedException()
        {
        }

        public EntityLockedException(string message)
            : base(message)
        {
        }

        public EntityLockedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        private EntityLockedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
   
}
