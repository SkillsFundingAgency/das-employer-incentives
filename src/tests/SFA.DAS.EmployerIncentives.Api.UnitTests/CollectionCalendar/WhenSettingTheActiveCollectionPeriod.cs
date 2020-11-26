using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Api.Controllers;
using SFA.DAS.EmployerIncentives.Api.Types;
using SFA.DAS.EmployerIncentives.Commands.CollectionPeriod;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Api.UnitTests.CollectionCalendar
{
    [TestFixture]
    public class WhenSettingTheActiveCollectionPeriod
    {
        private CollectionCalendarCommandController _sut;
        private Mock<ICommandDispatcher> _mockCommandDispatcher;

        [SetUp]
        public void Setup()
        {
            _mockCommandDispatcher = new Mock<ICommandDispatcher>();
            _sut = new CollectionCalendarCommandController(_mockCommandDispatcher.Object);

            _mockCommandDispatcher
                .Setup(m => m.Send(It.IsAny<ActivateCollectionPeriodCommand>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Test]
        public async Task Then_the_required_collection_calendar_period_is_set_to_active()
        {
            var request = new ActivateCollectionPeriodRequest { CollectionPeriodYear = 2020, CollectionPeriodNumber = 1 };
            await _sut.ActivateCollectionPeriod(request);

            _mockCommandDispatcher.Verify(m => m.Send(It.Is<ActivateCollectionPeriodCommand>(
                x => x.CollectionPeriodNumber == request.CollectionPeriodNumber 
                && x.CollectionPeriodYear == request.CollectionPeriodYear),
                It.IsAny<CancellationToken>()), Times.Once);
        }
    }

}
