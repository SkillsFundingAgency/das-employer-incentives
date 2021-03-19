@activeCalendarPeriod
Feature: MonthEndInProgress
	In order to maintain data consistency
	As employer incentives service
	I want actions to be suspended when month end is in progress

@mytag
Scenario: The month end payment process is started
	Given the active collection period is not currently in progress
	When the payment process is run
	Then the active collection period is set to in progress