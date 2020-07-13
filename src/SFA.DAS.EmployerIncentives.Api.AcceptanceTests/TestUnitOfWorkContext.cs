using SFA.DAS.UnitOfWork.Context;
using System;
using System.Collections.Generic;

namespace SFA.DAS.EmployerIncentives.Api.AcceptanceTests
{
    public class TestUnitOfWorkContext : IUnitOfWorkContext
    {
        private readonly TestContext _context;

        public TestUnitOfWorkContext(TestContext context)
        {
            _context = context;
        }

        public void AddEvent<T>(T message) where T : class
        {
            var eventList =  _context.TestData.GetOrCreate(onCreate: () => new List<T>());
            eventList.Add(message);
        }

        public void AddEvent<T>(Func<T> messageFactory) where T : class
        {
            throw new NotImplementedException();
        }
     
        public T Find<T>() where T : class
        {
            throw new NotImplementedException();
        }

        public T Get<T>() where T : class
        {
            throw new NotImplementedException();
        }

        public IEnumerable<object> GetEvents()
        {
            throw new NotImplementedException();
        }

        public void Set<T>(T value) where T : class
        {
            throw new NotImplementedException();
        }
    }
}
