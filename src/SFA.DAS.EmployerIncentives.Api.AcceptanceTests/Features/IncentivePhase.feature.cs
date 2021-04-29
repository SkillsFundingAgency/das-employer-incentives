﻿// ------------------------------------------------------------------------------
//  <auto-generated>
//      This code was generated by SpecFlow (https://www.specflow.org/).
//      SpecFlow Version:3.5.0.0
//      SpecFlow Generator Version:3.5.0.0
// 
//      Changes to this file may cause incorrect behavior and will be lost if
//      the code is regenerated.
//  </auto-generated>
// ------------------------------------------------------------------------------
#region Designer generated code
#pragma warning disable
namespace SFA.DAS.EmployerIncentives.Api.AcceptanceTests.Features
{
    using TechTalk.SpecFlow;
    using System;
    using System.Linq;
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("TechTalk.SpecFlow", "3.5.0.0")]
    [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [NUnit.Framework.TestFixtureAttribute()]
    [NUnit.Framework.DescriptionAttribute("IncentivePhase")]
    [NUnit.Framework.CategoryAttribute("database")]
    [NUnit.Framework.CategoryAttribute("api")]
    [NUnit.Framework.CategoryAttribute("domainMessageHandlers")]
    [NUnit.Framework.CategoryAttribute("messageBus")]
    [NUnit.Framework.CategoryAttribute("activeCalendarPeriod")]
    public partial class IncentivePhaseFeature
    {
        
        private TechTalk.SpecFlow.ITestRunner testRunner;
        
        private string[] _featureTags = new string[] {
                "database",
                "api",
                "domainMessageHandlers",
                "messageBus",
                "activeCalendarPeriod"};
        
#line 1 "IncentivePhase.feature"
#line hidden
        
        [NUnit.Framework.OneTimeSetUpAttribute()]
        public virtual void FeatureSetup()
        {
            testRunner = TechTalk.SpecFlow.TestRunnerManager.GetTestRunner();
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "Features", "IncentivePhase", "\tWhen an application is submitted then the phase of the application is stored aga" +
                    "inst the application and the created incentive", ProgrammingLanguage.CSharp, new string[] {
                        "database",
                        "api",
                        "domainMessageHandlers",
                        "messageBus",
                        "activeCalendarPeriod"});
            testRunner.OnFeatureStart(featureInfo);
        }
        
        [NUnit.Framework.OneTimeTearDownAttribute()]
        public virtual void FeatureTearDown()
        {
            testRunner.OnFeatureEnd();
            testRunner = null;
        }
        
        [NUnit.Framework.SetUpAttribute()]
        public virtual void TestInitialize()
        {
        }
        
        [NUnit.Framework.TearDownAttribute()]
        public virtual void TestTearDown()
        {
            testRunner.OnScenarioEnd();
        }
        
        public virtual void ScenarioInitialize(TechTalk.SpecFlow.ScenarioInfo scenarioInfo)
        {
            testRunner.OnScenarioInitialize(scenarioInfo);
            testRunner.ScenarioContext.ScenarioContainer.RegisterInstanceAs<NUnit.Framework.TestContext>(NUnit.Framework.TestContext.CurrentContext);
        }
        
        public virtual void ScenarioStart()
        {
            testRunner.OnScenarioStart();
        }
        
        public virtual void ScenarioCleanup()
        {
            testRunner.CollectScenarioErrors();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Incentive Application is submitted")]
        [NUnit.Framework.TestCaseAttribute("2020-08-01", "2021-06-01", "NotSet", null)]
        [NUnit.Framework.TestCaseAttribute("2020-08-01", "2021-05-31", "Phase1_0", null)]
        [NUnit.Framework.TestCaseAttribute("2020-08-02", "2021-05-31", "Phase1_0", null)]
        [NUnit.Framework.TestCaseAttribute("2021-01-31", "2021-05-31", "Phase1_0", null)]
        [NUnit.Framework.TestCaseAttribute("2021-02-01", "2021-05-31", "Phase1_1", null)]
        [NUnit.Framework.TestCaseAttribute("2021-02-02", "2021-05-31", "Phase1_1", null)]
        [NUnit.Framework.TestCaseAttribute("2021-03-31", "2021-05-31", "Phase1_1", null)]
        [NUnit.Framework.TestCaseAttribute("2021-04-01", "2021-05-31", "NotSet", null)]
        public virtual void IncentiveApplicationIsSubmitted(string startDate, string submissionDate, string phase, string[] exampleTags)
        {
            string[] tagsOfScenario = exampleTags;
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            argumentsOfScenario.Add("StartDate", startDate);
            argumentsOfScenario.Add("SubmissionDate", submissionDate);
            argumentsOfScenario.Add("Phase", phase);
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Incentive Application is submitted", null, tagsOfScenario, argumentsOfScenario);
#line 9
this.ScenarioInitialize(scenarioInfo);
#line hidden
            bool isScenarioIgnored = default(bool);
            bool isFeatureIgnored = default(bool);
            if ((tagsOfScenario != null))
            {
                isScenarioIgnored = tagsOfScenario.Where(__entry => __entry != null).Where(__entry => String.Equals(__entry, "ignore", StringComparison.CurrentCultureIgnoreCase)).Any();
            }
            if ((this._featureTags != null))
            {
                isFeatureIgnored = this._featureTags.Where(__entry => __entry != null).Where(__entry => String.Equals(__entry, "ignore", StringComparison.CurrentCultureIgnoreCase)).Any();
            }
            if ((isScenarioIgnored || isFeatureIgnored))
            {
                testRunner.SkipScenario();
            }
            else
            {
                this.ScenarioStart();
#line 10
 testRunner.Given(string.Format("an employer is applying for an apprenticeship with a start date of \'{0}\'", startDate), ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line hidden
#line 11
 testRunner.When(string.Format("they submit the application on \'{0}\'", submissionDate), ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
#line 12
 testRunner.Then(string.Format("the apprenticeship incentive phase for the the application is \'{0}\'", phase), ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
#line 13
 testRunner.And(string.Format("the apprenticeship incentive is created with an incentive phase of \'{0}\'", phase), ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            }
            this.ScenarioCleanup();
        }
    }
}
#pragma warning restore
#endregion
