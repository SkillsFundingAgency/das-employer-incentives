﻿using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Api.AcceptanceTests.Hooks;
using SFA.DAS.EmployerIncentives.Data.UnitTests.TestHelpers;
using SFA.DAS.HashingService;
using System;
using System.Collections.Generic;
using System.IO;

namespace SFA.DAS.EmployerIncentives.Api.AcceptanceTests
{    
    public class TestContext
    {
        public DirectoryInfo TestDirectory { get; set; }
        public SqlDatabase SqlDatabase { get; set; }
        public EmployerIncentiveApi EmployerIncentiveApi { get; set; }
        public TestAccountApi AccountApi { get; set; }
        public TestData TestData { get; set; }
        public IHashingService HashingService { get; set; }
        public List<IHook> Hooks { get; set; }
        public List<object> EventsPublished { get; set; }
        public List<PublishedCommand> CommandsPublished { get; set; }
        public bool ThrowErrorAfterSendingEvent { get; set; } = false;

        public TestContext()
        {
            TestDirectory = new DirectoryInfo(Path.Combine(Directory.GetCurrentDirectory(), Guid.NewGuid().ToString()));
            if (!TestDirectory.Exists)
            {
                Directory.CreateDirectory(TestDirectory.FullName);
            }
            TestData = new TestData();
            HashingService = new HashingService.HashingService("46789BCDFGHJKLMNPRSTVWXY", "SFA: digital apprenticeship service");
            Hooks = new List<IHook>();
            EventsPublished = new List<object>();
            CommandsPublished = new List<PublishedCommand>();
        }
    }
}
