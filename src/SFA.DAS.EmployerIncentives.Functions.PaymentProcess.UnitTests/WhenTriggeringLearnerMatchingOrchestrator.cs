using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.Orchestrators;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentProcess.UnitTests
{
    public class WhenTriggeringLearnerMatchingOrchestrator
    {
        private const string FunctionName = "LearnerMatchingOrchestrator";
        private Mock<IDurableOrchestrationClient> orchestrationClientMock = new Mock<IDurableOrchestrationClient>();

        [Test]
        public async Task Then_LearnerMatchingOrchestrator_is_invoked()
        {
            await LearnerMatchingOrchestrator_HttpStart.Run(null, orchestrationClientMock.Object, Mock.Of<ILogger>());

            orchestrationClientMock.Verify(x => x.StartNewAsync(FunctionName, null), Times.Once);
        }
    }
}