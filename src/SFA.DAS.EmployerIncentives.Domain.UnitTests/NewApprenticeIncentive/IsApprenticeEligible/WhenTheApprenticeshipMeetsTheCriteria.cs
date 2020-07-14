using System;
using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.ValueObjects;

namespace SFA.DAS.EmployerIncentives.Domain.UnitTests.NewApprenticeIncentive.IsApprenticeEligible
{
    [TestFixture]
    public class WhenTheApprenticeshipMeetsTheCriteria
    {
        private Fixture _fixture;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();
        }

        [Test]
        public void Then_the_apprenticeship_is_eligible()
        {
            var apprenticeship = _fixture
                .Build<Apprenticeship>()
                .With(f => f.StartDate, new DateTime(2020, 8, 1))
                .With(f => f.IsApproved, true)
                .Create();

            var result = Domain.NewApprenticeIncentive.NewApprenticeIncentive.IsApprenticeshipEligible(apprenticeship);

            result.Should().BeTrue();
        }
    }
}
