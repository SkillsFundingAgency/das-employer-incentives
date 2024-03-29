﻿@database
@api
@domainMessageHandlers
@messageBus
@activeCalendarPeriod
Feature: ApprenticeshipIncentiveCreated
	When an application has been submitted
	Then an apprenticeship incentive is created for each apprentiveship in the applicaiton

Scenario: Incentive Application is submitted
	Given an employer is applying for the New Apprenticeship Incentive
	When they submit the application
	Then the apprenticeship incentive is created for the application

Scenario: Apprenticeship incentive is created
	Given an employer has submitted an application
	When the apprenticeship incentive is created for each apprenticeship in the application
	Then the earnings are calculated for each apprenticeship incentive

Scenario: Apprenticeship incentive earnings are calculated
	Given an apprenticeship incentive exists
	When the apprenticeship incentive earnings are calculated
	Then the pending payments are stored against the apprenticeship incentive

Scenario: Incentive application updated after earnings calculated
	Given an apprenticeship incentive earnings have been calculated
	When the earnings calculation against the apprenticeship incentive completes
	Then the incentive application is updated to record that the earnings have been calculated

Scenario: Apprenticeship incentive submitted within 21 days of first payment due date
	Given an apprenticeship incentive exists with a payment due within the delay period
	When the apprenticeship incentive earnings are calculated
	Then the first pending payment is due at the end of the delay period

Scenario: Incentive application is submitted for withdrawn apprenticeship
	Given an existing withdrawn incentive
	And an employer is re-applying for apprenticeship incentive
	When they submit the application
	Then the apprenticeship incentive is created for each apprenticeship in the application
	And original withdrawn incentive is retained
