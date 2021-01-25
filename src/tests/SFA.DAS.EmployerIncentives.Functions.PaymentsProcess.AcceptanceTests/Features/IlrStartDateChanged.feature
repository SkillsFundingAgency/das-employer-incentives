@activeCalendarPeriod
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

Scenario: Clawbacks - Start Date Change Of Circumstance has been triggered for existing incentive
	Given an apprenticeship incentive exists
	And an earning has been paid for an apprenticeship incentive application
	When the learner data is refreshed with a new valid start date for the apprenticeship incentive
	Then the paid earning is marked as requiring a clawback

Scenario: Clawbacks - if ineligible start date
	Given an apprenticeship incentive exists
	And an earning has been paid for an apprenticeship incentive application
	When the learner data is refreshed with a new invalid start date for the apprenticeship incentive
	Then the paid earning is marked as requiring a clawback

Scenario: Clawbacks - Delete unpaid earnings
	Given an apprenticeship incentive exists
	And an earning has not been paid for an apprenticeship incentive application
	When the learner data is refreshed with a new valid start date for the apprenticeship incentive
	Then the unpaid earning is deleted
	And all unpaid payment records are deleted

Scenario: Clawbacks - eligible start date, create new first pending payment
	Given an apprenticeship incentive exists
	And an earning has been paid for an apprenticeship incentive application
	When the learner data is refreshed with a new valid start date for the apprenticeship incentive
	Then earnings are recalculated
	And a new pending first payment record is created
	And a new pending second payment record is created
