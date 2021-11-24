using System;

namespace SFA.DAS.EmployerIncentives.Abstractions.Logging
{    
    public class RequestLogWithArgs
    {
        public Func<Tuple<string, object[]>> OnProcessing { get; set; }
        public Func<Tuple<string, object[]>> OnError { get; set; }

        public RequestLogWithArgs()
        {
            OnProcessing = () => new Tuple<string, object[]>("", null);        
            OnError = () => new Tuple<string, object[]>("", null);
        }
    }
}
