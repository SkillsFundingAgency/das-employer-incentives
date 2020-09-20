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
	When the apprenticeship incentice is created for each apprenticship in the application
	Then the earnings are calculated for each apprenticeship incentice

Scenario: Apprenticeship incentive earnings are calculated
	Given an apprenticeship incentive exists
	When the apprenticeship incentice earnings are calculated
	Then the pending payments are stored against the apprenticeship incentive

Scenario: Incentive application updated after earnings calculated
	Given an apprenticeship incentive earnings have been calculated
	When the earnings calculation against the apprenticeship incentice completes
	Then the incentive application is updated to record that the earnings have been calculated