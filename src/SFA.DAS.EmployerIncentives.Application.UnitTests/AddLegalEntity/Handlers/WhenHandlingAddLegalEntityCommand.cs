using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Application.Commands;
using SFA.DAS.EmployerIncentives.Application.Commands.AddLegalEntity;
using SFA.DAS.EmployerIncentives.Application.Exceptions;
using SFA.DAS.EmployerIncentives.Application.Persistence;
using SFA.DAS.EmployerIncentives.Domain.Data;
using SFA.DAS.EmployerIncentives.Domain.Entities;
using SFA.DAS.EmployerIncentives.Domain.Interfaces;
using SFA.DAS.NServiceBus;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Application.UnitTests.AddLegalEntity.Handlers
{
    public class WhenHandlingAddLegalEntityCommand
    {
        private AddLegalEntityCommandHandler _sut;
        private Mock<IValidator<AddLegalEntityCommand>> _mockValidator;
        private Mock<IDomainRepository<long, Account>> _mockDomainRespository;
        
        private Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();

            _mockValidator = new Mock<IValidator<AddLegalEntityCommand>>();
            _mockValidator
                .Setup(m => m.Validate(It.IsAny<AddLegalEntityCommand>()))
                .ReturnsAsync(new ValidationResult());

            _mockDomainRespository = new Mock<IDomainRepository<long, Account>>();

            _sut = new AddLegalEntityCommandHandler(_mockValidator.Object, _mockDomainRespository.Object);
        }

        [Test]
        public async Task Then_the_command_is_validated()
        {
            //Arrange
            var command = _fixture.Create<AddLegalEntityCommand>();

            //Act
            await _sut.Handle(command);

            //Assert
            _mockValidator.Verify(m => m.Validate(command), Times.Once);
        }

        [Test]
        public void Then_an_InvalidRequestException_is_raised_when_the_command_is_not_valid()
        {
            //Arrange
            var command = _fixture.Create<AddLegalEntityCommand>();
            
            var validationResult = new ValidationResult();
            validationResult.AddError(_fixture.Create<string>());
            _mockValidator
               .Setup(m => m.Validate(It.IsAny<AddLegalEntityCommand>()))
               .ReturnsAsync(validationResult);

            //Act
            Func<Task> action = async () => await _sut.Handle(command);

            //Assert
            action.Should().Throw<InvalidRequestException>();
        }

        [Test]
        public async Task Then_the_a_new_account_is_persisted_to_the_domain_repository()
        {
            //Arrange
            var command = _fixture.Create<AddLegalEntityCommand>();

            //Act
            await _sut.Handle(command);

            //Assert
            _mockDomainRespository.Verify(m => m.Save(It.Is<Account>(i => i.Id == command.AccountId)), Times.Once);
        }

        [Test]
        public async Task Then_the_the_account_is_not_persisted_to_the_domain_repository_if_it_already_exists()
        {
            //Arrange
            var command = _fixture.Create<AddLegalEntityCommand>();
            _mockDomainRespository
                .Setup(m => m.Find(command.AccountId))
                .ReturnsAsync(Account.Create(new AccountModel { Id = 1, LegalEntityModels = new Collection<ILegalEntityModel>() }));

            //Act
            await _sut.Handle(command);

            //Assert
            _mockDomainRespository.Verify(m => m.Save(It.Is<Account>(i => i.Id == command.AccountId)), Times.Never);
        }
    }
}
