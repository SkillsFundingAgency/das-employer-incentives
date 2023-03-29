using SFA.DAS.EmployerIncentives.Infrastructure.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using TechTalk.SpecFlow;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.AcceptanceTests.Bindings
{
    [Binding]
    public class PaymentProcessSettingsHook
    {
        [BeforeScenario(Order = 3)]
        public void InitialiseApplicationSettings(TestContext context)
        {
            context.PaymentProcessSettings = new PaymentProcessSettings
            {
                MetricsReportEmailList = new List<string>
                {
                    "metricsApprover1@email.com",
                    "metricsApprover2@email.com",
                    "metricsApprover3@email.com"
                }
            };
        }
    }
}
