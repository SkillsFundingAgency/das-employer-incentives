@database
@api
@domainMessageHandlers
@messageBus
@activeCalendarPeriod
Feature: MonthEndInProgress
	In order to maintain data consistency
	As employer incentives service
	I want actions to be suspended when month end is in progress

Scenario: Earnings calculation is deferred when payment process in progress.
	Given an apprenticeship incentive exists
	And the active collection period is currently in progress
	When an earnings calculation is requested
	Then the earnings calculation is deferred

Scenario: Earnings calculation is resumed when payment process in progress.
	Given an apprenticeship incentive exists
	And the active collection period is currently in progress
	And an earnings calculation is requested
	And the earnings calculation is deferred
	And the active collection period is currently not in progress
	When the earnings calculation request is resumed
	Then the earnings calculation is deferred

Scenario: Withdrawal request is deferred when payment process in progress.
	Given an apprenticeship incentive exists
	And the active collection period is currently in progress
	When an employer withdrawal is requested
	Then the employer withdrawal is deferred

Scenario: Compliance withdrawal request is deferred when payment process in progress.
	Given an apprenticeship incentive exists
	And the active collection period is currently in progress
	When a compliance withdrawal is requested
	Then the compliance withdrawal is deferred

Scenario: Validation override request is deferred when payment process in progress.
	Given an apprenticeship incentive exists
	And the active collection period is currently in progress
	When a validation override is requested
	Then the validation override is deferred