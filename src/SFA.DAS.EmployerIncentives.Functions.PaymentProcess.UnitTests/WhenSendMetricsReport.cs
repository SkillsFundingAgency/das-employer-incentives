using AutoFixture;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.PaymentProcess;
using SFA.DAS.EmployerIncentives.Functions.PaymentsProcess;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentProcess.UnitTests
{
    public class WhenSendMetricsReport
    {
        private Fixture _fixture;
        private SendMetricsReport _sut;
        private Mock<ICommandDispatcher> _mockCommandDispatcher;

        [SetUp]
        public void Setup()
        {
            _fixture = new Fixture();
            _mockCommandDispatcher = new Mock<ICommandDispatcher>();
            
            _sut = new SendMetricsReport(_mockCommandDispatcher.Object);
        }

        [Test]
        public async Task Then_command_is_called_to_match_the_learner()
        {
            var input = _fixture.Create<SendMetricsReportInput>();
            await _sut.Complete(input);

            _mockCommandDispatcher.Verify(
                x => x.Send(
                    It.Is<SendMetricsReportCommand>(p =>
                        p.CollectionPeriod.AcademicYear == input.CollectionPeriod.Year &&
                        p.CollectionPeriod.PeriodNumber == input.CollectionPeriod.Period), CancellationToken.None), Times.Once);
        }
    }
}