@activeCalendarPeriod
@activeCalendarPeriod
Feature: LearningStopped
	When the refreshed learner data contains a learning stopped change of circumstance
	Then the apprenticeship incentive is updated to a stopped state

Scenario: Learner data contains a learning stopped change
	Given an apprenticeship incentive exists
	And the learner data identifies the learner as not in learning anymore
	When the incentive learner data is refreshed
	Then the incentive is updated to stopped
	And the stopped change of circumstance is saved

Scenario: Apprenticeship has unpaid earnings after the stopped date
	Given an apprenticeship incentive exists
	And the learner data identifies the learner as not in learning anymore
	And the apprenticeship incentive has unpaid earnings after the stopped date
	When the incentive learner data is refreshed
	Then the incentive is updated to stopped
	And the existing pending payments are removed

Scenario: Apprenticeship has paid earnings after the stopped date
	Given an apprenticeship incentive exists
	And the learner data identifies the learner as not in learning anymore
	And the apprenticeship incentive has paid earnings after the stopped date
	When the incentive learner data is refreshed
	Then the incentive is updated to stopped
	And the existing paid pending payments are clawed back