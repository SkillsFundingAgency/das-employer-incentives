using AutoFixture;
using System.Collections.Generic;

namespace SFA.DAS.EmployerIncentives.Functions.LegalEntities.AcceptanceTests
{
    public class TestData
    {
        private readonly Dictionary<string, object> _testdata;
        private readonly Fixture _fixture;
        public TestData()
        {
            _testdata = new Dictionary<string, object>();
            _fixture = new Fixture();
        }

        public T GetOrCreate<T>()
        {
            if (!_testdata.ContainsKey(nameof(T)))
            {
                _testdata.Add(nameof(T), _fixture.Create<T>());
            }

            return (T)_testdata[nameof(T)];
        }
    }
}
