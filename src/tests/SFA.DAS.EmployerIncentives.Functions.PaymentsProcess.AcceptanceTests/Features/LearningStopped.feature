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

Scenario: Learner data contains a learning resumed change
	Given an apprenticeship incentive exists that has stopped learning
	And the learner data identifies the learner as in learning
	When the incentive learner data is refreshed
	Then the incentive is updated to active
	And the resumed change of circumstance is saved
	And the learner data resumed date is stored

Scenario: A learning resumed change updated the pending payment due dates with the break in learning
	Given an apprenticeship incentive exists that has stopped learning
	And the learner data identifies the learner as in leaning with a break in learning
	When the incentive learner data is refreshed
	Then the pending payment due dates include the break in learning