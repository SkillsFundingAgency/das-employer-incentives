using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

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

        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        private EntityLockedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
   
}
