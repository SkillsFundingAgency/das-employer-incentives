@activeCalendarPeriod
Feature: IlrStartDateChanged
	When the refreshed learner data contains an updated start date
	Then the apprenticeship incentive is updated

Scenario: Learner data contains a new start date within the parameters of the incentive scheme
	Given an apprenticeship incentive exists
	When the learner data is updated with new valid start date for the apprenticeship incentive
	And the incentive learner data is refreshed
	Then the actual start date is updated
	And the pending payments are recalculated for the apprenticeship incentive
	And the learner data is subsequently refreshed

Scenario: Learner data contains a new start date outside of the parameters of the incentive scheme
	Given an apprenticeship incentive exists
	When the learner data is updated with new invalid start date for the apprenticeship incentive
	And the incentive learner data is refreshed
	Then the actual start date is updated
	And the existing pending payments are removed

Scenario: Clawbacks - Start Date Change Of Circumstance with eligible start date
	Given an apprenticeship incentive exists
	And an earning has been paid for an apprenticeship incentive application
	When the learner data is updated with new valid start date for the apprenticeship incentive
	And the incentive learner data is refreshed
	Then earnings are recalculated
	And the paid earning is marked as requiring a clawback
	And a new pending first payment record is created
	And a new pending second payment record is created

Scenario: Clawbacks - Start Date Change Of Circumstance with ineligible start date
	Given an apprenticeship incentive exists
	And an earning has been paid for an apprenticeship incentive application
	When the learner data is updated with new invalid start date for the apprenticeship incentive
	And the incentive learner data is refreshed
	Then the paid earning is marked as requiring a clawback

Scenario: Clawbacks - Start Date Change Of Circumstance Delete unpaid earnings
	Given an apprenticeship incentive exists
	And an earning has not been paid for an apprenticeship incentive application
	When the learner data is updated with new valid start date for the apprenticeship incentive
	And the incentive learner data is refreshed
	Then the unpaid earning is archived
	And all unpaid payment records are archived
	And all pending payment validation results are archived

Scenario: Clawbacks - Start Date Change Of Circumstance with eligible start date changing learner's age from under to over 25 - paid earning
	Given an apprenticeship incentive exists
	And an earning has been paid for an apprenticeship incentive application
	When the learner data is updated with a new valid start date for the apprenticeship incentive making the learner over twenty five at start
	And the incentive learner data is refreshed
	Then earnings are recalculated
	And the paid earning is marked as requiring a clawback
	And a new pending first payment record is created with a new amount and payment period
	And a new pending second payment record is created with a new amount and payment period
	And existing payment record is retained
	And existing pending payment validation record is retained
