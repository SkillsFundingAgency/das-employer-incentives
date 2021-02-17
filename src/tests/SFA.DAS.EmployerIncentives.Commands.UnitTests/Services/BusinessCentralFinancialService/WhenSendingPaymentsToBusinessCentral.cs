using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Abstractions.DTOs.Queries.ApprenticeshipIncentives;
using SFA.DAS.EmployerIncentives.Commands.Services.BusinessCentralApi;
using SFA.DAS.EmployerIncentives.Enums;
using SFA.DAS.EmployerIncentives.UnitTests.Shared;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Commands.UnitTests.Services.BusinessCentralFinancialService
{
    public class WhenSendingPaymentsToBusinessCentral
    {
        private BusinessCentralFinancePaymentsService _sut;
        private TestHttpClient _httpClient;
        private Uri _baseAddress;
        private Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();
            _baseAddress = new Uri(@"http://localhost");
            _httpClient = new TestHttpClient(_baseAddress);

            _sut = new BusinessCentralFinancePaymentsService(_httpClient, 3, "XXX", false);
        }

        [Test]
        public async Task Then_the_payment_is_posted_successfully()
        {
            //Arrange
            _httpClient.SetUpPostAsAsync(System.Net.HttpStatusCode.Accepted);
            var payment = _fixture.Create<PaymentDto>();

            //Act
            await _sut.SendPaymentRequests(new List<PaymentDto> { payment });
        }

        [Test]
        public async Task Then_the_payment_is_posted_with_the_correct_content_type()
        {
            //Arrange
            _httpClient.SetUpPostAsAsync(System.Net.HttpStatusCode.Accepted);
            var payment = _fixture.Create<PaymentDto>();

            //Act
            await _sut.SendPaymentRequests(new List<PaymentDto> { payment });

            // Assert
            _httpClient.VerifyContentType("application/payments-data");
        }

        [TestCase(HttpStatusCode.InternalServerError)]
        [TestCase(HttpStatusCode.ServiceUnavailable)]
        [TestCase(HttpStatusCode.BadGateway)]
        public async Task Then_the_payment_is_posted_and_we_get_an_internal_error_from_business_central_api(HttpStatusCode statusCode)
        {
            //Arrange
            _httpClient.SetUpPostAsAsync(statusCode);
            var payment = _fixture.Create<PaymentDto>();

            //Act
            Func<Task> act = async () => await _sut.SendPaymentRequests(new List<PaymentDto> { payment });

            act.Should().Throw<BusinessCentralApiException>().WithMessage("Business Central API is unavailable and returned an internal*");
        }

        [TestCase(HttpStatusCode.BadRequest)]
        [TestCase(HttpStatusCode.NotFound)]
        [TestCase(HttpStatusCode.Unauthorized)]
        [TestCase(HttpStatusCode.Forbidden)]
        public async Task Then_the_payment_is_posted_and_we_get_an_access_error_from_business_central_api(HttpStatusCode statusCode)
        {
            //Arrange
            _httpClient.SetUpPostAsAsync(statusCode);
            var payment = _fixture.Create<PaymentDto>();

            //Act
            Func<Task> act = async () => await _sut.SendPaymentRequests(new List<PaymentDto> { payment });

            act.Should().Throw<BusinessCentralApiException>().WithMessage("Business Central API returned*");
        }

        [Test]
        public void Then_the_payment_fields_are_mapped_correctly()
        {

            var payment = _fixture.Build<PaymentDto>().Create();

            var paymentRequest = _sut.MapToBusinessCentralPaymentRequest(payment);

            paymentRequest.RequestorUniquePaymentIdentifier.Should().Be(payment.PaymentId.ToString("N"));
            paymentRequest.Requestor.Should().Be("ApprenticeServiceEI");
            paymentRequest.FundingStream.Code.Should().Be("EIAPP");
            paymentRequest.FundingStream.StartDate.Should().Be("2020-09-01");
            paymentRequest.FundingStream.EndDate.Should().Be("2021-08-30");
            paymentRequest.DueDate.Should().Be(payment.DueDate.ToString("yyyy-MM-dd"));
            paymentRequest.VendorNo.Should().Be(payment.VendorId);
            paymentRequest.CostCentreCode.Should().Be("AAA40");
            paymentRequest.Amount.Should().Be(payment.Amount);
            paymentRequest.Currency.Should().Be("GBP");
            paymentRequest.ExternalReference.Type.Should().Be("ApprenticeIdentifier");
            paymentRequest.ExternalReference.Value.Should().Be(payment.HashedLegalEntityId);
        }

        [TestCase(SubnominalCode.Levy16To18, "2240147")]
        [TestCase(SubnominalCode.Levy19Plus, "2340147")]
        [TestCase(SubnominalCode.NonLevy16To18, "2240250")]
        [TestCase(SubnominalCode.NonLevy19Plus, "2340292")]
        public void Then_the_SubnominalCodes_are_mapped_to_accountcode(SubnominalCode subnominalCode, string expectedAccountCode)
        {

            var payment = _fixture.Build<PaymentDto>().With(x => x.SubnominalCode, subnominalCode).Create();

            var paymentRequest = _sut.MapToBusinessCentralPaymentRequest(payment);

            paymentRequest.AccountCode.Should().Be(expectedAccountCode);
        }

        [TestCase(EarningType.FirstPayment, "XXX", 12345, "Hire a new apprentice (first payment). Employer: XXX ULN: 12345")]
        [TestCase(EarningType.SecondPayment, "XXX", 12345, "Hire a new apprentice (second payment). Employer: XXX ULN: 12345")]
        public void Then_the_PaymentLineDescription_is_constructed_as_expected(EarningType earningType, string hashedLegalEntityId, long uln, string expected)
        {

            var payment = _fixture.Build<PaymentDto>()
                .With(x => x.EarningType, earningType)
                .With(x => x.HashedLegalEntityId, hashedLegalEntityId)
                .With(x => x.ULN, uln)
                .Create();

            var paymentRequest = _sut.MapToBusinessCentralPaymentRequest(payment);

            paymentRequest.PaymentLineDescription.Should().Be(expected);
        }

        [TestCase(1234567890, "******7890")]
        [TestCase(123456789, "*****6789")]
        public void Then_the_PaymentLineDescription_is_constructed_with_uln_obfuscated(long uln, string expected)
        {

            _sut = new BusinessCentralFinancePaymentsService(_httpClient, 3, "XXX", true);

            var payment = _fixture.Build<PaymentDto>()
                .With(x => x.EarningType, EarningType.FirstPayment)
                .With(x => x.HashedLegalEntityId, "ABCD")

                .With(x => x.ULN, uln)
                .Create();

            var paymentRequest = _sut.MapToBusinessCentralPaymentRequest(payment);

            paymentRequest.PaymentLineDescription.Should().Be($"Hire a new apprentice (first payment). Employer: ABCD ULN: {expected}");
        }
    }
}
