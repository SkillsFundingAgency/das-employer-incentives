using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Domain.Data;
using SFA.DAS.EmployerIncentives.Domain.Entities;
using System;

namespace SFA.DAS.EmployerIncentives.Domain.UnitTests.AccountTests
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
            var account = Account.New(id);

            // Assert
            account.Id.Should().Be(id);
        }

        [Test]
        public void Then_errors_if_the_model_is_null()
        {
            // Arrange

            // Act
            Action action = () => Account.Create(null);

            // Assert
            action.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void Then_errors_if_the_model_id_is_not_set()
        {
            // Arrange

            // Act
            Action action = () => Account.Create(new AccountModel());

            // Assert
            action.Should().Throw<ArgumentException>().Where(e => e.Message.StartsWith("Id is not set"));
        }
    }
}
