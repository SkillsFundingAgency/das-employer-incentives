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
                .Setup(m => m.Send(It.IsAny<UpdateCollectionPeriodCommand>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Test]
        public async Task Then_the_required_collection_calendar_period_is_set_to_active()
        {
            var request = new CollectionPeriod { AcademicYear = 2021, PeriodNumber = 1 };
            await _sut.UpdateCollectionPeriod(request);

            _mockCommandDispatcher.Verify(m => m.Send(It.Is<UpdateCollectionPeriodCommand>(
                x => x.PeriodNumber == request.PeriodNumber 
                && x.AcademicYear == request.AcademicYear),
                It.IsAny<CancellationToken>()), Times.Once);
        }
    }

}
