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