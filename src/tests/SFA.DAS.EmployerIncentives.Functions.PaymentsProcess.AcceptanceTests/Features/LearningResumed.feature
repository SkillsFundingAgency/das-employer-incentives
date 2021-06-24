@activeCalendarPeriod
Feature: LearningResumed
	When the refreshed learner data contains a learning resumed change of circumstance
	Then the apprenticeship incentive is updated to an active state and required earnings are calculated

Scenario: Calculate earnings when Learner data contains a learning resumed change for an incentive
	Given an apprenticeship incentive in a stopped state exists
	And the stopped date was '<WhenStopped>' the original first payment due date
	And there are '<FirstPendingPayment>' first earnings
	When the incentive learner data is refreshed
	Then the incentive is updated to active
	And the resumed change of circumstance is saved
	And the learner data resumed date is stored
	And the first payment due date is '<IsCalculated>' to include the break in learning

Examples:
	| WhenStopped | FirstPendingPayment | IsCalculated  |
	| Before      | No                  | Calculated    |
	| On          | Unpaid              | NotCalculated |
	| After       | Unpaid              | NotCalculated |
	| Before      | ClawedBack          | Calculated    |
	| On          | Paid                | NotCalculated |
	| After       | Paid                | NotCalculated |
