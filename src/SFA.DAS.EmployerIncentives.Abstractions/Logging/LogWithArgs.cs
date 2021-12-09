using System;

namespace SFA.DAS.EmployerIncentives.Abstractions.Logging
{  
    public class LogWithArgs
    {
        public Func<Tuple<string, object[]>> OnProcessing { get; set; }
        public Func<Tuple<string, object[]>> OnProcessed { get; set; }
        public Func<Tuple<string, object[]>> OnError { get; set; }

        public LogWithArgs()
        {
            OnProcessing = () => new Tuple<string, object[]>("", null);
            OnProcessed = () => new Tuple<string, object[]>("", null);
            OnError = () => new Tuple<string, object[]>("", null);
        }
    }
}
