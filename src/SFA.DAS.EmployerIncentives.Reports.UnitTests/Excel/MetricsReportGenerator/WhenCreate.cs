using AutoFixture;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Data.Reports.Metrics;
using SFA.DAS.EmployerIncentives.Reports.Excel.Metrics;
using System.IO;
using FluentAssertions;

namespace SFA.DAS.EmployerIncentives.Reports.UnitTests.Excel.MetricsReportGeneratorTests
{
    public class WhenCreate
    {
        private MetricsReportGenerator _sut;
        private Fixture _fixture;
        private MetricsReport _metricsReport;

        [SetUp]
        public void Setup()
        {
            _fixture = new Fixture();

            _metricsReport = _fixture.Create<MetricsReport>();
            _sut = new MetricsReportGenerator();
        }

        [Test]
        public void Then_the_report_stream_is_created()
        {
            // Arrange

            // Act
            var streamResult = _sut.Create(_metricsReport);

            // Assert
            streamResult.Should().BeAssignableTo<MemoryStream>();
            streamResult.Length.Should().BeGreaterThan(8800);            
        }
    }
}
