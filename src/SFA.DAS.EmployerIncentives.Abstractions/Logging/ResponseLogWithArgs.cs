using System;

namespace SFA.DAS.EmployerIncentives.Abstractions.Logging
{    
    public class ResponseLogWithArgs
    {
        public Func<Tuple<string, object[]>> OnProcessed { get; set; }

        public ResponseLogWithArgs()
        {
            OnProcessed = () => new Tuple<string, object[]>("", null);
        }
    }
}
