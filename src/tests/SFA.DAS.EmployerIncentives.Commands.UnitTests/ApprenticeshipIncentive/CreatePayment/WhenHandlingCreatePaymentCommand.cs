using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.Common.Domain.Types;
using SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.CreatePayment;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
using SFA.DAS.EmployerIncentives.Domain.Accounts.Models;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;
using SFA.DAS.EmployerIncentives.Domain.Factories;
using SFA.DAS.EmployerIncentives.Domain.ValueObjects;
using SFA.DAS.EmployerIncentives.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LegalEntity = SFA.DAS.EmployerIncentives.Domain.Accounts.LegalEntity;

namespace SFA.DAS.EmployerIncentives.Commands.UnitTests.ApprenticeshipIncentive.CreatePayment
{
    public class WhenHandlingCreatePaymentCommand
    {
        private CreatePaymentCommandHandler _sut;
        private Mock<IApprenticeshipIncentiveDomainRepository> _mockIncentiveDomainRespository;
        private Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();

            _mockIncentiveDomainRespository = new Mock<IApprenticeshipIncentiveDomainRepository>();

            _fixture.Register(ApprenticeshipIncentiveCreator);

            _sut = new CreatePaymentCommandHandler(_mockIncentiveDomainRespository.Object);
        }

        [Test]
        public async Task Then_a_payment_is_created()
        {
            //Arrange
            var incentive = _fixture.Create<Domain.ApprenticeshipIncentives.ApprenticeshipIncentive>();

            var command = new CreatePaymentCommand(incentive.Id, incentive.PendingPayments.First().Id, _fixture.Create<short>(), _fixture.Create<byte>());

            _mockIncentiveDomainRespository.Setup(x => x.Find(command.ApprenticeshipIncentiveId)).ReturnsAsync(incentive);

            // Act
            await _sut.Handle(command);

            // Assert
            incentive.Payments.Count.Should().Be(1);
        }

        [Test]
        public async Task Then_the_payment_is_persisted()
        {
            //Arrange
            var incentive = _fixture.Create<Domain.ApprenticeshipIncentives.ApprenticeshipIncentive>();

            var command = new CreatePaymentCommand(incentive.Id, incentive.PendingPayments.First().Id, _fixture.Create<short>(), _fixture.Create<byte>());

            _mockIncentiveDomainRespository.Setup(x => x.Find(command.ApprenticeshipIncentiveId)).ReturnsAsync(incentive);

            int itemsPersisted = 0;
            _mockIncentiveDomainRespository.Setup(m => m.Save(It.Is<Domain.ApprenticeshipIncentives.ApprenticeshipIncentive>(a => a.Id == command.ApprenticeshipIncentiveId)))
                                            .Callback(() =>
                                            {
                                                itemsPersisted++;
                                            });


            // Act
            await _sut.Handle(command);

            // Assert
            itemsPersisted.Should().Be(1);
        }

        private Domain.ApprenticeshipIncentives.ApprenticeshipIncentive ApprenticeshipIncentiveCreator()
        {
            var incentive = new ApprenticeshipIncentiveFactory()
                .CreateNew(_fixture.Create<Guid>(),
                    _fixture.Create<Guid>(),
                    _fixture.Create<Account>(),
                    new Apprenticeship(
                        _fixture.Create<long>(),
                        _fixture.Create<string>(),
                        _fixture.Create<string>(),
                        DateTime.Today.AddYears(-26),
                        _fixture.Create<long>(),
                        ApprenticeshipEmployerType.Levy
                    ),
                    DateTime.Today);

            var paymentProfiles = new List<IncentivePaymentProfile>
            {
                new IncentivePaymentProfile(
                    IncentiveType.TwentyFiveOrOverIncentive, new List<PaymentProfile>
                    {
                        new PaymentProfile(10, 100),
                        new PaymentProfile(100, 1000)
                    })
            };

            var collectionPeriods = new List<Domain.ValueObjects.CollectionPeriod>()
            {
                new Domain.ValueObjects.CollectionPeriod(1, (byte)DateTime.Now.Month, (short)DateTime.Now.Year, DateTime.Now.AddDays(-1)
                    ,DateTime.Now, DateTime.Now.Year.ToString(), true)
            };

            incentive.CalculateEarnings(paymentProfiles, new Domain.ValueObjects.CollectionCalendar(collectionPeriods));
            var account = Domain.Accounts.Account.New(incentive.Account.Id);
            var legalEntityModel = _fixture.Build<LegalEntityModel>().With(x => x.AccountLegalEntityId, incentive.PendingPayments.First().Account.AccountLegalEntityId).With(x => x.VrfVendorId, "kjhdfhjksdfg").Create();
            account.AddLegalEntity(incentive.PendingPayments.First().Account.AccountLegalEntityId, LegalEntity.Create(legalEntityModel));
            incentive.ValidatePendingPaymentBankDetails(incentive.PendingPayments.First().Id, account, collectionPeriods.First());

            return incentive;
        }
    }
}
