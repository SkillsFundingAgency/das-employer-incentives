@activeCalendarPeriod
Feature: IlrStartDateChanged
	When the refreshed learner data contains an updated start date
	Then the apprenticeship incentive is updated

Scenario: Learner data contains a new start date within the parameters of the incentive scheme
	Given a '<Phase>' apprenticeship incentive exists
	When the learner data is updated with new valid start date for the apprenticeship incentive
	And the incentive learner data is refreshed
	Then the actual start date is updated
	And the pending payments are recalculated for the apprenticeship incentive
	And the learner data is subsequently refreshed
Examples:
	| Phase  |
	| Phase1 |
	| Phase2 |

Scenario: Learner data contains a new start date outside of the parameters of the incentive scheme
	Given a '<Phase>' apprenticeship incentive exists
	When the learner data is updated with new invalid start date for the apprenticeship incentive
	And the incentive learner data is refreshed
	Then the actual start date is updated
	And the existing pending payments are removed
Examples:
	| Phase  |
	| Phase1 |
	| Phase2 |

Scenario: Learner data contains a new start date change of circumstance
	Given a '<Phase>' apprenticeship incentive exists
	When the learner data is updated with new valid start date for the apprenticeship incentive
	And the incentive learner data is refreshed
	Then the start date change of circumstance is saved
	And the minimum agreement version is changed to '<AgreementVersion>'
Examples:
	| Phase  | AgreementVersion |
	| Phase1 | 5                |
	| Phase2 | 6                |

Scenario: Start date results in first earning due within the delay period
	Given a 'Phase2' apprenticeship incentive exists
	When the learner data is updated with new start date which will create earning in the delay period
	And the incentive learner data is refreshed
	Then earnings are recalculated
	And a new first earning with a due date at the end of the delay period is created