using NUnit.Framework;
using TechTalk.SpecFlow;

namespace SFA.DAS.EmployerIncentives.Functions.LegalEntities.AcceptanceTests.Steps
{
    [Binding, Scope(Tag = "Function")]
    public class LegalEntityCreatedSteps : StepsBase
    {
        private readonly TestData _testData;

        public LegalEntityCreatedSteps(TestContext testContext) : base(testContext)
        {
            _testData = testContext.TestData;
        }

        [Given(@"I have a legal entity that is not in the database")]
        public void GivenIHaveALegalEntityThatIsNew()
        {
            Assert.Inconclusive("Not yet implemented");
        }

        [Given(@"I have a legal entity that is already in the database")]
        public void GivenIHaveALegalEntityThatAlreadyExists()
        {
            Assert.Inconclusive("Not yet implemented");
        }

        [Given(@"I have an invalid legal entity that is new")]
        public void GivenIHaveAnInvalidLegalEntityThatIsNew()
        {
            Assert.Inconclusive("Not yet implemented");
        }

        [Given(@"I have a legal entity that is invalid")]
        public void GivenIHaveALegalEntityThatIsInvalid()
        {
            Assert.Inconclusive("Not yet implemented");
        }

        [When(@"added legal entity event is triggered")]
        public void WhenAddedLegalEntityEventIsTriggered()
        {
            Assert.Inconclusive("Not yet implemented");
        }
        
        [Then(@"the legal entity should be available")]
        public void ThenTheLegalEntityShouldBeAvailable()
        {            
            Assert.Inconclusive("Not yet implemented");
        }                

        [Then(@"the legal entity should not be available")]
        public void ThenTheLegalEntityShouldNotBeAvailable()
        {
            Assert.Inconclusive("Not yet implemented");
        }
    }
}
