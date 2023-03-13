using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Data.Reports;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Data.UnitTests.AccountDataRepository
{
    public class WhenExecuteCalled
    {
        private ReportsDataRepository _sut;
        private Mock<IReportsConnectionProvider> _reportsConnectionProvider;

        [SetUp]
        public void Arrange()
        {
            _reportsConnectionProvider = new Mock<IReportsConnectionProvider>();
            _sut = new ReportsDataRepository(_reportsConnectionProvider.Object);
        }

        [Test]
        public async Task Then_null_is_returned_when_the_report_isnt_supported()
        {
            // Arrange

            // Act
            var result = await _sut.Execute<TestReport>();

            // Assert
            result.Should().BeNull();            
        }

        protected class TestReport : IReport
        {
            public string Name => "TestName";
        }
    }
}
