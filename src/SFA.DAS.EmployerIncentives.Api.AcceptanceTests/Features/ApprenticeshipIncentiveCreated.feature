@database
@api
@domainMessageHandlers
Feature: ApprenticeshipIncentiveCreated
	When an application has been submitted
	Then an apprenticeship incentive is created for each apprentiveship in the applicaiton

Scenario: Incentive Application is submitted
	Given an employer is applying for the New Apprenticeship Incentive
	When they submit the application
	Then the apprentiveship incentive is created for the application