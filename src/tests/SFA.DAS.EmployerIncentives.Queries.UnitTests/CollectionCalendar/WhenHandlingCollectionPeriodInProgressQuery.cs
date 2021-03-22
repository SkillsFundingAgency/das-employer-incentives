using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Domain.Interfaces;
using SFA.DAS.EmployerIncentives.Domain.ValueObjects;
using SFA.DAS.EmployerIncentives.Queries.CollectionCalendar.CollectionPeriodInProgress;

namespace SFA.DAS.EmployerIncentives.Queries.UnitTests.CollectionCalendar
{
    [TestFixture]
    public class WhenHandlingCollectionPeriodInProgressQuery
    {
        private Mock<ICollectionCalendarService> _service;
        private CollectionPeriodInProgressQueryHandler _sut;
        private CollectionPeriod _activeCollectionPeriod;

        [SetUp]
        public void SetUp()
        {
            _service = new Mock<ICollectionCalendarService>();
            _sut = new CollectionPeriodInProgressQueryHandler(_service.Object);

            _activeCollectionPeriod = new CollectionPeriod(2, 2021);
            _activeCollectionPeriod.SetActive(true);

            var calendarPeriods = new List<CollectionPeriod>
            {
                new CollectionPeriod(1, 2021),
                _activeCollectionPeriod,
                new CollectionPeriod(3, 2021)
            };

            var collectionCalendar = new Domain.ValueObjects.CollectionCalendar(calendarPeriods);
            _service.Setup(x => x.Get()).ReturnsAsync(collectionCalendar);
        }

        [TestCase(true)]
        [TestCase(false)]
        public async Task ThenActiveCollectionPeriodInProgressStatusIsReturned(bool isInProgress)
        {
            _activeCollectionPeriod.SetPeriodEndInProgress(isInProgress);

            var response = await _sut.Handle(new CollectionPeriodInProgressRequest(), CancellationToken.None);

            response.IsInProgress.Should().Be(isInProgress);
        }
    }
}
