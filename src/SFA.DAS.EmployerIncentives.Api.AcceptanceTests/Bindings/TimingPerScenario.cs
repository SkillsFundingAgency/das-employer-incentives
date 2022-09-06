using System;
using System.Diagnostics;
using TechTalk.SpecFlow;

namespace SFA.DAS.EmployerIncentives.Api.AcceptanceTests.Bindings
{
    [Binding]
    public class TimingPerScenario
    {
        private Stopwatch _stopWatch;

        [BeforeScenario(Order = 0)]
        public void Start()
        {
            _stopWatch = new Stopwatch();
            _stopWatch.Start();
        }

        [AfterScenario(Order = 100)]
        public void Stop(ScenarioInfo scenarioInfo)
        {
            _stopWatch.Stop();
            Console.WriteLine($"TESTRUN: Time Taken for test '{scenarioInfo.Title}' = {_stopWatch.ElapsedMilliseconds}ms");
        }
    }
}
