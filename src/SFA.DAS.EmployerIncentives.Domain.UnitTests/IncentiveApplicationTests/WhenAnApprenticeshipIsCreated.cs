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

        [TestCase(null, "2021-10-01", false)]
        [TestCase("2021-03-31", "2021-04-01", false)]
        [TestCase("2021-04-01 00:00:00", "2021-04-01", false)]
        [TestCase("2021-04-01 23:59:59", "2021-04-01", false)]
        [TestCase("2021-04-01", "2021-04-01", false)]
        [TestCase("2021-11-30", "2021-10-01", true)]
        [TestCase("2022-01-31 23:59:59", "2021-10-01", true)]
        [TestCase("2022-02-01", "2021-10-01", false)]

        public void Then_the_has_eligible_employer_start_is_set_correctly_based_on_the_employer_start_date(DateTime? employerStartDate, DateTime commitmentStartDate, bool isEligible)
        {   
            var apprenticeship = new IncentiveApplicationFactory().CreateApprenticeship(_fixture.Create<long>(), _fixture.Create<string>(), _fixture.Create<string>(), _fixture.Create<DateTime>(), _fixture.Create<long>(), commitmentStartDate, ApprenticeshipEmployerType.Levy, _fixture.Create<long>(), _fixture.Create<string>(), employerStartDate);

            apprenticeship.StartDatesAreEligible.Should().Be(isEligible);
        }

        private Apprenticeship CreateApprenticeship(DateTime startDate, DateTime dateOfBirth)
        {
            var apprenticeship = new IncentiveApplicationFactory().CreateApprenticeship(_fixture.Create<long>(), _fixture.Create<string>(), _fixture.Create<string>(), dateOfBirth, _fixture.Create<long>(), startDate, ApprenticeshipEmployerType.Levy, _fixture.Create<long>(), _fixture.Create<string>(), _fixture.Create<DateTime>());
            return apprenticeship;
        }

    }
}
