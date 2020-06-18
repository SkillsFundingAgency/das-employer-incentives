using AutoFixture;
using FluentAssertions;
using NUnit.Framework;

namespace SFA.DAS.EmployerIncentives.Domain.UnitTests.Account
{
    public class WhenCreated
    {
        private Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();
        }

        [Test]
        public void Then_the_id_is_set()
        {
            // Arrange
            var id = _fixture.Create<long>();

            // Act
            var account = Domain.Entities.Account.New(id);

            // Assert
            account.Id.Should().Be(id);
        }
    }
}
