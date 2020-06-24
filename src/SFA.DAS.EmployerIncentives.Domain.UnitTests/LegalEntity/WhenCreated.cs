using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Domain.Accounts;
using SFA.DAS.EmployerIncentives.Domain.Accounts.Models;
using System;

namespace SFA.DAS.EmployerIncentives.Domain.UnitTests.LegalEntityTests
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
            var legalEntity = LegalEntity.New(id, _fixture.Create<string>());

            // Assert
            legalEntity.Id.Should().Be(id);
        }

        [Test]
        public void Then_the_name_is_set()
        {
            // Arrange
            var name = _fixture.Create<string>();

            // Act
            var legalEntity = LegalEntity.New(_fixture.Create<long>(), name);

            // Assert
            legalEntity.Name.Should().Be(name);
        }

        [Test]
        public void Then_errors_if_the_model_is_null()
        {
            // Arrange

            // Act
            Action action = () => LegalEntity.Create(null);

            // Assert
            action.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void Then_errors_if_the_model_id_is_not_set()
        {
            // Arrange

            // Act
            Action action = () => LegalEntity.Create(new LegalEntityModel());

            // Assert
            action.Should().Throw<ArgumentException>().Where(e => e.Message.StartsWith("Id is not set"));
        }
    }
}
