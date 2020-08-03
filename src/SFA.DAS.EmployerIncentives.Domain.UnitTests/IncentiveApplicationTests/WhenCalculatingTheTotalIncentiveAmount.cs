using System;
using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Common.Domain.Types;
using SFA.DAS.EmployerIncentives.Domain.Factories;

namespace SFA.DAS.EmployerIncentives.Domain.UnitTests.IncentiveApplicationTests
{
    public class WhenCalculatingTheTotalIncentiveAmount
    {
        private Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();
        }

        [Test]
        public void Then_the_24_or_under_total_is_returned_if_the_apprentice_is_exactly_24_at_the_start_date()
        {
            var startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month));
            var dateOfBirth = startDate.AddYears(-25).AddDays(1);

            var appenticeship = new IncentiveApplicationFactory().CreateNewApprenticeship(_fixture.Create<Guid>(), _fixture.Create<int>(), _fixture.Create<string>(), _fixture.Create<string>(), dateOfBirth, _fixture.Create<long>(), startDate, ApprenticeshipEmployerType.Levy);

            appenticeship.TotalIncentiveAmount.Should().Be(2000);
        }

        [Test]
        public void Then_the_24_or_under_total_is_returned_if_the_apprentice_is_under_24_at_the_start_date()
        {
            var startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month));
            var dateOfBirth = startDate.AddYears(-18);

            var appenticeship = new IncentiveApplicationFactory().CreateNewApprenticeship(_fixture.Create<Guid>(), _fixture.Create<int>(), _fixture.Create<string>(), _fixture.Create<string>(), dateOfBirth, _fixture.Create<long>(), startDate, ApprenticeshipEmployerType.Levy);

            appenticeship.TotalIncentiveAmount.Should().Be(2000);
        }

        [Test]
        public void Then_the_25_or_over_total_is_returned_if_the_apprentice_is_exactly_25_at_the_start_date()
        {
            var startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month));
            var dateOfBirth = startDate.AddYears(-25);

            var appenticeship = new IncentiveApplicationFactory().CreateNewApprenticeship(_fixture.Create<Guid>(), _fixture.Create<int>(), _fixture.Create<string>(), _fixture.Create<string>(), dateOfBirth, _fixture.Create<long>(), startDate, ApprenticeshipEmployerType.Levy);

            appenticeship.TotalIncentiveAmount.Should().Be(1500);
        }

        [Test]
        public void Then_the_25_or_over_total_is_returned_if_the_apprentice_is_over_25_at_the_start_date()
        {
            var startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month));
            var dateOfBirth = startDate.AddYears(-26);

            var appenticeship = new IncentiveApplicationFactory().CreateNewApprenticeship(_fixture.Create<Guid>(), _fixture.Create<int>(), _fixture.Create<string>(), _fixture.Create<string>(), dateOfBirth, _fixture.Create<long>(), startDate, ApprenticeshipEmployerType.Levy);

            appenticeship.TotalIncentiveAmount.Should().Be(1500);
        }
    }
}
