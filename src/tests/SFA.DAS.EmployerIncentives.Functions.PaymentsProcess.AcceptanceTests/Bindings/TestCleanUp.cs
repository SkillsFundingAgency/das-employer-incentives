﻿using System;
using System.IO;
using TechTalk.SpecFlow;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.AcceptanceTests.Bindings
{
    [Binding]
    public class TestCleanUp
    {
        private readonly TestContext _context;

        public TestCleanUp(TestContext context)
        {         
            _context = context;
        }

        [BeforeTestRun(Order = 1)]
        public static void DeleteTestDirectory()
        {
            try
            {
                DeleteDirectory(new DirectoryInfo(Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.Parent.Parent.FullName, "TestDirectory")).FullName);
            }
            catch (Exception) 
            {
                // ignore
            }
        }

        [AfterScenario(Order = 100)]
        public void CleanUp()
        {
            try
            {
                Directory.Delete(_context.TestDirectory.FullName, true);
            }
            catch(Exception)
            {
                // ignore
            }
        }

        private static void DeleteDirectory(string directory)
        {
            string[] files = Directory.GetFiles(directory);
            string[] directories = Directory.GetDirectories(directory);

            foreach (string file in files)
            {
                File.SetAttributes(file, FileAttributes.Normal);
                File.Delete(file);
            }

            foreach (string dir in directories)
            {
                DeleteDirectory(dir);
            }

            Directory.Delete(directory, false);
        }
    }
}