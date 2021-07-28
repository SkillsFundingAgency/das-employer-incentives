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

Scenario: Learner match does not run when payment process in progress.
	Given an apprenticeship incentive exists
	And the active collection period is currently in progress
	When the learner match process is run
	Then the learner match data is not updated