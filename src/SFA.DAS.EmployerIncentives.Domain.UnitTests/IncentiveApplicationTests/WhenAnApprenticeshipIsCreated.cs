using System;
using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Common.Domain.Types;
using SFA.DAS.EmployerIncentives.Domain.Factories;
using SFA.DAS.EmployerIncentives.Domain.IncentiveApplications;

namespace SFA.DAS.EmployerIncentives.Domain.UnitTests.IncentiveApplicationTests
{
    public class WhenAnApprenticeshipIsCreated
    {
        private Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();
        }

        [Test]
        public void Then_the_total_incentive_amount_is_the_24_or_under_total_if_the_apprentice_is_one_day_under_25_at_the_start_date()
        {
            var startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month));
            var dateOfBirth = startDate.AddYears(-25).AddDays(1);

            var apprenticeship = CreateApprenticeship(startDate, dateOfBirth);

            apprenticeship.TotalIncentiveAmount.Should().Be(2000);
        }

        [Test]
        public void Then_the_total_incentive_amount_is_the_24_or_under_total_if_the_apprentice_is_under_24_at_the_start_date()
        {
            var startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month));
            var dateOfBirth = startDate.AddYears(-18);

            var apprenticeship = CreateApprenticeship(startDate, dateOfBirth);

            apprenticeship.TotalIncentiveAmount.Should().Be(2000);
        }

        [Test]
        public void Then_the_total_incentive_amount_is_the_25_or_over_total_if_the_apprentice_is_exactly_25_at_the_start_date()
        {
            var startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month));
            var dateOfBirth = startDate.AddYears(-25);

            var apprenticeship = CreateApprenticeship(startDate, dateOfBirth);

            apprenticeship.TotalIncentiveAmount.Should().Be(1500);
        }

        [Test]
        public void Then_the_total_incentive_amount_is_the_25_or_over_total_if_the_apprentice_is_over_25_at_the_start_date()
        {
            var startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month));
            var dateOfBirth = startDate.AddYears(-26);

            var apprenticeship = CreateApprenticeship(startDate, dateOfBirth);

            apprenticeship.TotalIncentiveAmount.Should().Be(1500);
        }

        private Apprenticeship CreateApprenticeship(DateTime startDate, DateTime dateOfBirth)
        {
            var apprenticeship = new IncentiveApplicationFactory().CreateApprenticeship(_fixture.Create<long>(), _fixture.Create<string>(), _fixture.Create<string>(), dateOfBirth, _fixture.Create<long>(), startDate, ApprenticeshipEmployerType.Levy, _fixture.Create<long>(), _fixture.Create<string>());
            return apprenticeship;
        }
    }
}
