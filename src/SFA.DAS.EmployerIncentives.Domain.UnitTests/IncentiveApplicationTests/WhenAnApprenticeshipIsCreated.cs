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

        [TestCase(null, false)]
        [TestCase("2021-03-31", false)]
        [TestCase("2021-04-01 00:00:00", true)]
        [TestCase("2021-04-01 23:59:59", true)]
        [TestCase("2021-04-01", true)]
        [TestCase("2021-09-30", true)]
        [TestCase("2021-09-30 23:59:59", true)]
        [TestCase("2021-10-01", false)]

        public void Then_the_has_eligible_employer_start_is_set_correctly_based_on_the_employer_start_date(DateTime? employerStartDate, bool isEligible)
        {   
            var apprenticeship = new IncentiveApplicationFactory().CreateApprenticeship(_fixture.Create<long>(), _fixture.Create<string>(), _fixture.Create<string>(), _fixture.Create<DateTime>(), _fixture.Create<long>(), _fixture.Create<DateTime>(), ApprenticeshipEmployerType.Levy, _fixture.Create<long>(), _fixture.Create<string>(), employerStartDate);

            apprenticeship.HasEligibleEmploymentStartDate.Should().Be(isEligible);
        }

        private Apprenticeship CreateApprenticeship(DateTime startDate, DateTime dateOfBirth)
        {
            var apprenticeship = new IncentiveApplicationFactory().CreateApprenticeship(_fixture.Create<long>(), _fixture.Create<string>(), _fixture.Create<string>(), dateOfBirth, _fixture.Create<long>(), startDate, ApprenticeshipEmployerType.Levy, _fixture.Create<long>(), _fixture.Create<string>(), _fixture.Create<DateTime>());
            return apprenticeship;
        }

    }
}
