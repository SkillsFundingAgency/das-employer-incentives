@database
@api
@domainMessageHandlers
@messageBus
Feature: ApprenticeshipIncentiveCreated
	When an application has been submitted
	Then an apprenticeship incentive is created for each apprentiveship in the applicaiton

Scenario: Incentive Application is submitted
	Given an employer is applying for the New Apprenticeship Incentive
	When they submit the application
	Then the apprentiveship incentive is created for the application

Scenario: Apprenticeship incentive is created
	Given an employer has submitted an application
	When the incentive is created for the application
	Then an apprenticeship incentice is created for each apprenticship in the application