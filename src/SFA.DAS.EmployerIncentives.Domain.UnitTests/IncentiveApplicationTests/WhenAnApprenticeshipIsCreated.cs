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

        [TestCase(18)]
        [TestCase(24)]
        [TestCase(25)]
        [TestCase(42)]
        public void Then_the_total_incentive_amount_is_the_expected_amount_regardless_of_the_apprentices_age(int ageInYears)
        {
            var startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month));
            var dateOfBirth = startDate.AddYears(-1 * ageInYears);

            var apprenticeship = CreateApprenticeship(startDate, dateOfBirth);

            apprenticeship.TotalIncentiveAmount.Should().Be(3000);
        }

        [TestCase(null, false)]
        [TestCase("2021-03-31", false)]
        [TestCase("2021-04-01 00:00:00", true)]
        [TestCase("2021-04-01 23:59:59", true)]
        [TestCase("2021-04-01", true)]
        [TestCase("2021-11-30", true)]
        [TestCase("2022-01-31 23:59:59", true)]
        [TestCase("2022-02-01", false)]

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
