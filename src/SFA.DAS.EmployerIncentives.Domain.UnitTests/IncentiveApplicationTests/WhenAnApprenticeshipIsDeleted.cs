using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Domain.IncentiveApplications;
using SFA.DAS.EmployerIncentives.UnitTests.Shared.AutoFixtureCustomizations;
using System.Linq;

namespace SFA.DAS.EmployerIncentives.Domain.UnitTests.IncentiveApplicationTests
{
    public class WhenAnApprenticeshipIsDeleted
    {
        private IncentiveApplication _sut;
        private Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();
            _fixture.Customize(new IncentiveApplicationCustomization());

            _sut = _fixture.Create<IncentiveApplication>();
        }

        [Test]
        public void Then_the_apprenticeship_is_deleted()
        {
            // Arrange
            var deleted = _fixture.Create<Apprenticeship>();
            _sut.AddApprenticeship(deleted);
            _sut.AddApprenticeship(_fixture.Create<Apprenticeship>());
            _sut.AddApprenticeship(_fixture.Create<Apprenticeship>());
            _sut.AddApprenticeship(_fixture.Create<Apprenticeship>());

            // Act
            _sut.RemoveApprenticeship(deleted);

            // Assert
            _sut.Apprenticeships.Count.Should().Be(3);
            _sut.Apprenticeships.Any(x => x.Id == deleted.Id).Should().BeFalse();
        }
    }
}
