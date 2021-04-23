using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
using SFA.DAS.EmployerIncentives.Domain.Interfaces;
using SFA.DAS.EmployerIncentives.Domain.ValueObjects;
using SFA.DAS.EmployerIncentives.Enums;
using LegalEntity = SFA.DAS.EmployerIncentives.Domain.Accounts.LegalEntity;

namespace SFA.DAS.EmployerIncentives.Commands.UnitTests.ApprenticeshipIncentive.CreatePayment
{
    public class WhenHandlingCreatePaymentCommand
    {
        private CreatePaymentCommandHandler _sut;
        private Mock<IApprenticeshipIncentiveDomainRepository> _mockIncentiveDomainRespository;
        private Mock<ICollectionCalendarService> _mockCollectionCalendarService;
        private Mock<IIncentivePaymentProfilesService> _mockIncentivePaymentProfilesService;
        private Fixture _fixture;
        private List<Domain.ValueObjects.CollectionPeriod> _collectionPeriods;
        private Domain.ValueObjects.CollectionPeriod _firstCollectionPeriod;
        private Domain.ApprenticeshipIncentives.ApprenticeshipIncentive _incentive;

        [SetUp]
        public async Task ArrangeAsync()
        {
            var today = new DateTime(2021, 1, 30);

            _fixture = new Fixture();

            _collectionPeriods = new List<Domain.ValueObjects.CollectionPeriod>()
            {
                new Domain.ValueObjects.CollectionPeriod(
                    1,
                    (byte)today.Month,
                    (short)today.Year,
                    today.AddDays(-1),
                    today.AddDays(-1),
                    (short)today.Year,
                    false),
                new Domain.ValueObjects.CollectionPeriod(
                1,
                (byte)today.AddMonths(1).Month,
                (short)today.AddMonths(1).Year,
                today.AddMonths(1).AddDays(-1),
                today.AddMonths(1).AddDays(-1),
                (short)today.AddMonths(1).Year,
                false)
            };
            _firstCollectionPeriod = _collectionPeriods.First();

            _mockIncentiveDomainRespository = new Mock<IApprenticeshipIncentiveDomainRepository>();
            _mockCollectionCalendarService = new Mock<ICollectionCalendarService>();
            _mockIncentivePaymentProfilesService = new Mock<IIncentivePaymentProfilesService>();

            _incentive = await ApprenticeshipIncentiveCreator();

            _sut = new CreatePaymentCommandHandler(_mockIncentiveDomainRespository.Object);
        }

        [Test]
        public async Task Then_a_payment_is_created()
        {
            //Arrange
            var command = new CreatePaymentCommand(_incentive.Id, _incentive.PendingPayments.First().Id,
                _firstCollectionPeriod.CalendarYear, _firstCollectionPeriod.PeriodNumber);

            _mockIncentiveDomainRespository.Setup(x => x.Find(command.ApprenticeshipIncentiveId)).ReturnsAsync(_incentive);

            // Act
            await _sut.Handle(command);

            // Assert
            _incentive.Payments.Count.Should().Be(1);
        }

        [Test]
        public async Task Then_the_payment_is_persisted()
        {
            //Arrange
            var command = new CreatePaymentCommand(_incentive.Id, _incentive.PendingPayments.First().Id, _firstCollectionPeriod.CalendarYear, _firstCollectionPeriod.PeriodNumber);

            _mockIncentiveDomainRespository.Setup(x => x.Find(command.ApprenticeshipIncentiveId)).ReturnsAsync(_incentive);

            int itemsPersisted = 0;
            _mockIncentiveDomainRespository.Setup(m => m.Save(It.Is<Domain.ApprenticeshipIncentives.ApprenticeshipIncentive>( a => a.Id == command.ApprenticeshipIncentiveId)))
                                            .Callback(() =>
                                            {
                                                itemsPersisted++;
                                            });

            // Act
            await _sut.Handle(command);

            // Assert
            itemsPersisted.Should().Be(1);
        }

        private async Task<Domain.ApprenticeshipIncentives.ApprenticeshipIncentive> ApprenticeshipIncentiveCreator()
        {
            var today = new DateTime(2021, 1, 30);

            var incentive = new ApprenticeshipIncentiveFactory()
                .CreateNew(_fixture.Create<Guid>(),
                    _fixture.Create<Guid>(),
                    _fixture.Create<Account>(),
                    new Apprenticeship(
                        _fixture.Create<long>(),
                        _fixture.Create<string>(),
                        _fixture.Create<string>(),
                        today.AddYears(-26),
                        _fixture.Create<long>(),
                        ApprenticeshipEmployerType.Levy,
                        _fixture.Create<string>()
                    ),
                    today,
                    _fixture.Create<DateTime>(),
                    _fixture.Create<string>(),
                    _fixture.Create<int>());

            incentive.Apprenticeship.SetProvider(_fixture.Create<Provider>());

            var paymentProfiles = new List<IncentivePaymentProfile>
            {
                new IncentivePaymentProfile(
                    IncentivePhase.Phase1_0,
                    4,
                    DateTime.MinValue,
                    DateTime.MaxValue,
                    DateTime.MinValue,
                    DateTime.MaxValue,
                    new List<PaymentProfile>
                    {
                        new PaymentProfile(10, 100,IncentiveType.TwentyFiveOrOverIncentive, EarningType.FirstPayment),
                        new PaymentProfile(100, 1000,IncentiveType.TwentyFiveOrOverIncentive, EarningType.SecondPayment)
                    })
            };

            var collectionPeriods = new List<Domain.ValueObjects.CollectionPeriod>()
            {
                new Domain.ValueObjects.CollectionPeriod(
                    1, 
                    (byte)today.Month, 
                    (short)today.Year,
                    today.AddDays(-1),
                    today,
                    (short)today.Year,
                    true)
            };

            _mockCollectionCalendarService.Setup(m => m.Get()).ReturnsAsync(new Domain.ValueObjects.CollectionCalendar(collectionPeriods));
            _mockIncentivePaymentProfilesService.Setup(m => m.Get()).ReturnsAsync(new IncentiveProfiles(paymentProfiles));

            await incentive.CalculateEarnings(_mockIncentivePaymentProfilesService.Object, _mockCollectionCalendarService.Object);

            var account = Domain.Accounts.Account.New(incentive.Account.Id);
            var legalEntityModel = _fixture.Build<LegalEntityModel>().With(x => x.AccountLegalEntityId, incentive.PendingPayments.First().Account.AccountLegalEntityId).With(x => x.VrfVendorId, "kjhdfhjksdfg").Create();
            account.AddLegalEntity(incentive.PendingPayments.First().Account.AccountLegalEntityId, LegalEntity.Create(legalEntityModel));
            incentive.ValidatePendingPaymentBankDetails(incentive.PendingPayments.First().Id, account, _collectionPeriods.First());

            return incentive;
        }
    }
}
