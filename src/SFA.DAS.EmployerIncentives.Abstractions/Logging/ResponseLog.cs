using System;

namespace SFA.DAS.EmployerIncentives.Abstractions.Logging
{
    public class ResponseLog
    {
        public Func<string> OnProcessed { get; set; }

        public ResponseLog()
        {
            OnProcessed = () => "";
        }
    }
}
