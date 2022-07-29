using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Commands.Services.BusinessCentralApi;
using SFA.DAS.EmployerIncentives.Enums;
using SFA.DAS.EmployerIncentives.UnitTests.Shared;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SFA.DAS.EmployerIncentives.Application.UnitTests;
using SFA.DAS.EmployerIncentives.DataTransferObjects.Queries.ApprenticeshipIncentives;

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
            var payment = _fixture.Create<Payment>();

            //Act
            await _sut.SendPaymentRequests(new List<Payment> { payment });
        }

        [Test]
        public async Task Then_the_nonsensitive_payment_data_is_logged()
        {
            //Arrange
            var loggerMock = new Mock<ILogger<BusinessCentralFinancePaymentsServiceWithLogging>>();
            _sut = new BusinessCentralFinancePaymentsService(_httpClient, 3, "XXX", false);
            var decorator = new BusinessCentralFinancePaymentsServiceWithLogging(_sut, loggerMock.Object, false);
            _httpClient.SetUpPostAsAsync(HttpStatusCode.Accepted);
            var payment1 = _fixture.Create<Payment>();
            var payment2 = _fixture.Create<Payment>();
            var req1 = payment1.Map(false);
            var req2 = payment2.Map(false);

            //Act
            await decorator.SendPaymentRequests(new List<Payment> { payment1, payment2 });

            //Assert
            loggerMock.VerifyLogContains(LogLevel.Information, Times.Once(), req1.ActivityCode);
            loggerMock.VerifyLogContains(LogLevel.Information, Times.Once(), req1.AccountCode);
            loggerMock.VerifyLogContains(LogLevel.Information, Times.Once(), req1.CostCentreCode);
            loggerMock.VerifyLogContains(LogLevel.Information, Times.Once(), req1.RequestorUniquePaymentIdentifier);
            loggerMock.VerifyLogContains(LogLevel.Information, Times.Once(), req1.DueDate);
            loggerMock.VerifyLogContains(LogLevel.Information, Times.Once(), req2.ActivityCode);
            loggerMock.VerifyLogContains(LogLevel.Information, Times.Once(), req2.AccountCode);
            loggerMock.VerifyLogContains(LogLevel.Information, Times.Once(), req2.CostCentreCode);
            loggerMock.VerifyLogContains(LogLevel.Information, Times.Once(), req2.RequestorUniquePaymentIdentifier);
            loggerMock.VerifyLogContains(LogLevel.Information, Times.Once(), req2.DueDate);
        }

        [Test]
        public async Task Then_the_payment_is_posted_with_the_correct_content_type()
        {
            //Arrange
            _httpClient.SetUpPostAsAsync(System.Net.HttpStatusCode.Accepted);
            var payment = _fixture.Create<Payment>();

            //Act
            await _sut.SendPaymentRequests(new List<Payment> { payment });

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
            var payment = _fixture.Create<Payment>();

            //Act / Assert
            try
            {
                await _sut.SendPaymentRequests(new List<Payment> { payment });
            }
            catch(BusinessCentralApiException exception)
            {
                exception.Message.Should().StartWith("Business Central API is unavailable and returned an internal");
                var delimiter = "Data sent: ";
                var json = exception.Message.Substring(exception.Message.IndexOf(delimiter) + delimiter.Length - 1);
                var paymentRequests = JsonConvert.DeserializeObject<PaymentRequestContainer>(json);
                foreach(var paymentRequest in paymentRequests.PaymentRequests)
                {
                    paymentRequest.DueDate.Should().NotBeNull();
                    paymentRequest.RequestorUniquePaymentIdentifier.Should().NotBeNull();
                    paymentRequest.AccountCode.Should().NotBeNull();
                    paymentRequest.CostCentreCode.Should().NotBeNull();
                    paymentRequest.ActivityCode.Should().NotBeNull();
                    paymentRequest.Amount.Should().Be(0);
                    paymentRequest.Approver.Should().BeNullOrEmpty();
                    paymentRequest.Currency.Should().BeNullOrEmpty();
                    paymentRequest.ExternalReference.Should().BeNull();
                    paymentRequest.FundingStream.Should().BeNull();
                    paymentRequest.PaymentLineDescription.Should().BeNullOrEmpty();
                    paymentRequest.Requestor.Should().BeNullOrEmpty();
                }
            }
        }

        [TestCase(HttpStatusCode.BadRequest)]
        [TestCase(HttpStatusCode.NotFound)]
        [TestCase(HttpStatusCode.Unauthorized)]
        [TestCase(HttpStatusCode.Forbidden)]
        public Task Then_the_payment_is_posted_and_we_get_an_access_error_from_business_central_api(HttpStatusCode statusCode)
        {
            //Arrange
            _httpClient.SetUpPostAsAsync(statusCode);
            var payment = _fixture.Create<Payment>();

            //Act
            Func<Task> act = async () => await _sut.SendPaymentRequests(new List<Payment> { payment });

            return act.Should().ThrowAsync<BusinessCentralApiException>().WithMessage("Business Central API returned*");
        }

        [Test]
        public void Then_the_payment_fields_are_mapped_correctly()
        {

            var payment = _fixture.Build<Payment>().Create();

            var paymentRequest = payment.Map(false);

            paymentRequest.RequestorUniquePaymentIdentifier.Should().Be(payment.PaymentId.ToString("N"));
            paymentRequest.Requestor.Should().Be("ApprenticeServiceEI");
            paymentRequest.FundingStream.Code.Should().Be("EIAPP");
            paymentRequest.FundingStream.StartDate.Should().Be("2020-09-01");
            paymentRequest.FundingStream.EndDate.Should().Be("2021-08-30");
            paymentRequest.DueDate.Should().Be(DateTime.UtcNow.ToString("yyyy-MM-dd"));
            paymentRequest.VendorNo.Should().Be(payment.VendorId);
            paymentRequest.CostCentreCode.Should().Be("10233");
            paymentRequest.Amount.Should().Be(payment.Amount);
            paymentRequest.Currency.Should().Be("GBP");
            paymentRequest.ExternalReference.Type.Should().Be("ApprenticeIdentifier");
            paymentRequest.ExternalReference.Value.Should().Be(payment.HashedLegalEntityId);
        }

        [TestCase(SubnominalCode.Levy16To18, "54156003")]
        [TestCase(SubnominalCode.Levy19Plus, "54156002")]
        [TestCase(SubnominalCode.NonLevy16To18, "54156003")]
        [TestCase(SubnominalCode.NonLevy19Plus, "54156002")]
        public void Then_the_SubnominalCodes_are_mapped_to_AccountCode(SubnominalCode subnominalCode, string expectedAccountCode)
        {
            var payment = _fixture.Build<Payment>().With(x => x.SubnominalCode, subnominalCode).Create();

            var paymentRequest = payment.Map(false);

            paymentRequest.AccountCode.Should().Be(expectedAccountCode);
        }

        [TestCase(SubnominalCode.Levy16To18, "100339")]
        [TestCase(SubnominalCode.Levy19Plus, "100388")]
        [TestCase(SubnominalCode.NonLevy16To18, "100349")]
        [TestCase(SubnominalCode.NonLevy19Plus, "100397")]
        public void Then_the_SubnominalCodes_are_mapped_to_ActivityCode(SubnominalCode subnominalCode, string expectedActivityCode)
        {
            var payment = _fixture.Build<Payment>().With(x => x.SubnominalCode, subnominalCode).Create();

            var paymentRequest = payment.Map(false);

            paymentRequest.ActivityCode.Should().Be(expectedActivityCode);
        }

        [TestCase(EarningType.FirstPayment, "XXX", 12345, "Hire a new apprentice (first payment). Employer: XXX ULN: 12345")]
        [TestCase(EarningType.SecondPayment, "XXX", 12345, "Hire a new apprentice (second payment). Employer: XXX ULN: 12345")]
        public void Then_the_PaymentLineDescription_is_constructed_as_expected(EarningType earningType, string hashedLegalEntityId, long uln, string expected)
        {

            var payment = _fixture.Build<Payment>()
                .With(x => x.EarningType, earningType)
                .With(x => x.HashedLegalEntityId, hashedLegalEntityId)
                .With(x => x.ULN, uln)
                .Create();

            var paymentRequest = payment.Map(false);

            paymentRequest.PaymentLineDescription.Should().Be(expected);
        }

        [TestCase(1234567890, "******7890")]
        [TestCase(123456789, "*****6789")]
        public void Then_the_PaymentLineDescription_is_constructed_with_uln_obfuscated(long uln, string expected)
        {
            _sut = new BusinessCentralFinancePaymentsService(_httpClient, 3, "XXX", true);

            var payment = _fixture.Build<Payment>()
                .With(x => x.EarningType, EarningType.FirstPayment)
                .With(x => x.HashedLegalEntityId, "ABCD")

                .With(x => x.ULN, uln)
                .Create();

            var paymentRequest = payment.Map(true);

            paymentRequest.PaymentLineDescription.Should().Be($"Hire a new apprentice (first payment). Employer: ABCD ULN: {expected}");
        }
    }
}
