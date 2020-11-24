Feature: IlrStartDateChanged
	When the refreshed learner data contains an updated start date
	Then the apprenticeship incentive is updated

Scenario: Learner data contains a new start date within the parameters of the incentive scheme
	Given an apprenticeship incentive exists
	When the learner data is refreshed with a new valid start date for the apprenticeship incentive
	Then the actual start date is updated
	And the pending payments are recalculated for the apprenticeship incentive
	And the learner data is subsequently refreshed

Scenario: Learner data contains a new start date outside of the parameters of the incentive scheme
	Given an apprenticeship incentive exists
	When the learner data is refreshed with a new invalid start date for the apprenticeship incentive
	Then the actual start date is updated
	And the existing pending payments are removed