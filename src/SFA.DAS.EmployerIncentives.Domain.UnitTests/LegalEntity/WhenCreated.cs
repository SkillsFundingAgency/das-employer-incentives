using AutoFixture;
using FluentAssertions;
using NUnit.Framework;

namespace SFA.DAS.EmployerIncentives.Domain.UnitTests.LegalEntity
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
            var legalEntity = Domain.Entities.LegalEntity.New(id, _fixture.Create<string>());

            // Assert
            legalEntity.Id.Should().Be(id);
        }

        [Test]
        public void Then_the_name_is_set()
        {
            // Arrange
            var name = _fixture.Create<string>();

            // Act
            var legalEntity = Domain.Entities.LegalEntity.New(_fixture.Create<long>(), name);

            // Assert
            legalEntity.Name.Should().Be(name);
        }
    }
}
