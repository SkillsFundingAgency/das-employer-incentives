@activeCalendarPeriod
Feature: StartDateChangedWithLearningStoppedCoC
	When the refreshed learner data contains a start date changed and alearning stopped change of circumstance
	Then the apprenticeship incentive data is updated

Scenario: Learner stopped date is before re-calculated due date as a result of a start date change
	Given an apprenticeship incentive exists
	And the learner data identifies the learner as having stopped CoC and a StartDate CoC
	And the stopped date is before the recalculated earning due date
	When the incentive learner data is refreshed
	Then both recalculated first and second earnings are deleted

Scenario: Learner stopped date is between re-calculated first and second due dates as a result of a start date change
	Given an apprenticeship incentive exists
	And the learner data identifies the learner as having stopped CoC and a StartDate CoC
	And the stopped date is between the recalculated earning first and second due dates
	When the incentive learner data is refreshed
	Then retain the first earnings
	And the second earning is deleted

Scenario: Learner stopped date is after re-calculated second due date as a result of a start date change
	Given an apprenticeship incentive exists
	And the learner data identifies the learner as having stopped CoC and a StartDate CoC
	And the stopped date is after the recalculated second due date
	When the incentive learner data is refreshed
	Then retain the first earnings
	And retain the second earnings
