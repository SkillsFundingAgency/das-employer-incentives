using AutoFixture;
using System;
using System.Collections.Generic;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.AcceptanceTests
{
    public class TestData
    {
        private readonly Dictionary<string, object> _testdata = new Dictionary<string, object>();
        private readonly Fixture _fixture = new Fixture();

        public T GetOrCreate<T>(string key = null, Func<T> onCreate = null)
        {
            key ??= typeof(T).FullName;

            if (!_testdata.ContainsKey(key))
            {
                _testdata.Add(key, onCreate == null ? _fixture.Create<T>() : onCreate.Invoke());
            }

            return (T)_testdata[key];
        }

        public T Get<T>(string key = null)
        {
            if (key == null)
            {
                key = typeof(T).FullName;
            }

            return (T)_testdata[key];
        }

        public void Set<T>(string key, T value)
        {
            _testdata[key] = value;
        }
    }
}
