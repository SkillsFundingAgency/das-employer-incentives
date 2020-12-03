using System;
using System.Collections.Generic;

namespace SFA.DAS.EmployerIncentives.Functions.TestHelpers
{
    public class OrchestrationStarterInfo
    {
        public string StarterName { get; private set; }
        public Dictionary<string, object> StarterArgs { get; private set; }
        public string OrchestrationName { get; private set; }
        public TimeSpan? Timeout { get; private set; }

        public OrchestrationStarterInfo(
            string starterName,
            string orchestrationName,
            Dictionary<string, object> args = null,
            TimeSpan? timeout = null)
        {
            if (string.IsNullOrEmpty(starterName)) throw new ArgumentException("Missing starter name");
            if (string.IsNullOrEmpty(orchestrationName)) throw new ArgumentException("Missing starter name");

            StarterName = starterName;
            OrchestrationName = orchestrationName;
            if (args == null)
            {
                args = new Dictionary<string, object>();
            }
            if (timeout == null)
            {
                timeout = new TimeSpan(0, 0, 30);
            }
            Timeout = timeout;
            StarterArgs = args;
        }
    }
}
