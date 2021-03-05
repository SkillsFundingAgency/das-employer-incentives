@activeCalendarPeriod
Feature: LearningStopped
	When the refreshed learner data contains a learning stopped change of circumstance
	Then the apprenticeship incentive is updated to a stopped state

Scenario: Learner data contains a learning stopped change
	Given an apprenticeship incentive exists
	And the learner data identifies the learner as not in leaning anymore
	When the incentive learner data is refreshed
	Then the incentive is updated to stopped
	And the stopped change of circumstance is saved
