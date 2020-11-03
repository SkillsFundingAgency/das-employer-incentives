using AutoFixture;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Functions.PaymentsProcess;
using SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.Orchestrators;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentProcess.UnitTests
{
    public class WhenTriggeringMonthEndOrchestrator
    {

        [Test]
        public async Task Then_MonthEndOrchestrator_is_invoked_with_passed_in_arguments()
        {
            // Arrange
            const string orchestratorName = "MonthEndOrchestrator";
            var orchestrationClientMock = new Mock<IDurableOrchestrationClient>();
            var fixture = new Fixture();
            var year = fixture.Create<short>();
            var month = fixture.Create<byte>();

            // Act
            await MonthEndOrchestrator_HttpStart.Run(null, orchestrationClientMock.Object, year, month, Mock.Of<ILogger>());

            // Assert
            orchestrationClientMock.Verify(x => x.StartNewAsync(orchestratorName, null,
                It.Is<CollectionPeriod>(p => p.Month == month && p.Year == year)), Times.Once);
        }
    }
}