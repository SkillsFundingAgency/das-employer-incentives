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
	And the learner data stopped date is stored
	And the learner start break in learning is stored

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

Scenario: Learner data contains a learning resumed change
	Given an apprenticeship incentive exists that has stopped learning
	And the learner data identifies the learner as in learning
	When the incentive learner data is refreshed
	Then the incentive is updated to active
	And the resumed change of circumstance is saved
	And the learner data resumed date is stored
	And the learner resume break in learning is stored
	
Scenario: Learner data contains a learning resumed change with start date before the start of assumed break in learning (defect EI-1195)
	Given an apprenticeship incentive exists that has stopped learning
	And the learner data identifies the learner as in learning with start date before recorded break date
	When the incentive learner data is refreshed
	Then the incentive is updated to active
	And the most recent break in learning record is deleted
	And the learner data stopped and resumed dates are deleted
