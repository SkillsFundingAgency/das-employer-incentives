using System;

namespace SFA.DAS.EmployerIncentives.Abstractions.Logging
{
    public class RequestLog
    {
        public Func<string> OnProcessing { get; set; }
        public Func<string> OnError { get; set; }

        public RequestLog()
        {
            OnProcessing = () => "";
            OnError = () => "";
        }
    }
}
