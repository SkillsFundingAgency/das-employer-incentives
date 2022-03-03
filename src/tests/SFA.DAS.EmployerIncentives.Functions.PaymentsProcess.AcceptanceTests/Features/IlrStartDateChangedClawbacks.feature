@activeCalendarPeriod
Feature: IlrStartDateChangedClawbacks
	When the refreshed learner data contains an updated start date
	Then the apprenticeship incentive is updated

Scenario: Clawbacks - Start Date Change Of Circumstance with eligible start date
	Given a '<Phase>' apprenticeship incentive exists
	And an earning has been paid for an apprenticeship incentive application
	When the learner data is updated with new valid start date for the apprenticeship incentive
	And the incentive learner data is refreshed
	Then earnings are recalculated
	And the paid earning is marked as requiring a clawback
	And a new first earning of '<FirstPaymentAmount>' is created
	And a new second earning of '<SecondPaymentAmount>' is created
Examples:
	| Phase  | FirstPaymentAmount | SecondPaymentAmount |
	| Phase1 | 750                | 750                 |
	| Phase2 | 1500               | 1500                |

Scenario: Clawbacks - Start Date Change Of Circumstance with ineligible start date
	Given a '<Phase>' apprenticeship incentive exists
	And an earning has been paid for an apprenticeship incentive application
	When the learner data is updated with new invalid start date for the apprenticeship incentive
	And the incentive learner data is refreshed
	Then the paid earning is marked as requiring a clawback
Examples:
	| Phase  |
	| Phase1 |
	| Phase2 |

Scenario: Clawbacks - Start Date Change Of Circumstance Delete unpaid earnings
	Given a '<Phase>' apprenticeship incentive exists
	And an earning has not been paid for an apprenticeship incentive application
	When the learner data is updated with new valid start date for the apprenticeship incentive
	And the incentive learner data is refreshed
	Then the unpaid earning is archived
	And all unpaid payment records are archived
	And all pending payment validation results are archived
Examples:
	| Phase  |
	| Phase1 |
	| Phase2 |

Scenario: Clawbacks - Start Date Change Of Circumstance with eligible start date changing learner's age from under to over 25 - paid earning
	Given a '<Phase>' apprenticeship incentive exists
	And an earning has been paid for an apprenticeship incentive application
	When the learner data is updated with a new valid start date for the apprenticeship incentive making the learner over twenty five at start
	And the incentive learner data is refreshed
	Then earnings are recalculated
	And the paid earning is marked as requiring a clawback
	And a new first earning of '<FirstPaymentAmount>' is created
	And a new second earning of '<SecondPaymentAmount>' is created
	And existing payment record is retained
	And existing pending payment validation record is retained
Examples:
	| Phase  | FirstPaymentAmount | SecondPaymentAmount |
	| Phase1 | 750                | 750                 |
	| Phase2 | 1500               | 1500                |
