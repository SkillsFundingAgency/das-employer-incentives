@activeCalendarPeriod
Feature: EmploymentCheck

Scenario: Employment checks requested on first ILR submission
	Given an apprenticeship incentive has been submitted in phase '<Phase>'
	And we have not previously requested an employment check for the learner
	And 6 weeks has elapsed since the start date of the apprenticeship
	When an ILR submission is received for that learner
	Then a new employment check is requested to ensure the apprentice was not employed in the 6 months prior to phase '<Phase>' starting
	And a new employment check is requested to ensure the apprentice was employed in the six weeks following their start date
	And a 365 employment check is not requested
Examples:
	| Phase  |
	| Phase1 |
	| Phase2 |
	| Phase3 |

Scenario: Employment checks requested on Start date change of circumstance
	Given an apprenticeship incentive has been submitted in phase '<Phase>'
	And we have previously requested an employment check for the learner
	And the learner data is updated with new valid start date for phase '<Phase>'
	And 6 weeks has elapsed since the start date of the apprenticeship
	When an ILR submission is received for that learner
	Then a new employment check is requested to ensure the apprentice was not employed in the 6 months prior to phase '<Phase>' starting
	And a new employment check is requested to ensure the apprentice was employed in the six weeks following their start date
	And a 365 employment check is not requested
Examples:
	| Phase  |
	| Phase1 |
	| Phase2 |
	| Phase3 |

Scenario: 365 Employment check requested for Phase 
	Given an apprenticeship incentive has been submitted in phase '<Phase>'
	And start of apprenticeship employment checks have passed
	And 3 weeks has elapsed since 365 days after the due date of the second payment
	When an ILR submission is received for that learner
	Then a new 365 employment check is requested to ensure the apprentice was employed when the second payment was due for the phase '<Phase>'
Examples:
	| Phase  |
	| Phase1 |
	| Phase2 |
	| Phase3 |

Scenario: 365 Employment check not re-requested for Phase after initial failure
	Given an apprenticeship incentive has been submitted in phase '<Phase>'	
	And start of apprenticeship employment checks have passed
	And 3 weeks has elapsed since 365 days after the due date of the second payment
	And the initial 365 check has failed
	When an ILR submission is received for that learner
	Then a re-request 365 employment check is not requested for the phase '<Phase>'
Examples:
	| Phase  |
	| Phase1 |
	| Phase2 |
	| Phase3 |

Scenario: 365 Employment check re-requested for Phase after initial failure
	Given an apprenticeship incentive has been submitted in phase '<Phase>'
	And start of apprenticeship employment checks have passed
	And 6 weeks has elapsed since 365 days after the due date of the second payment
	And the initial 365 check has failed
	When an ILR submission is received for that learner
	Then a re-request 365 employment check is requested for the phase '<Phase>'
Examples:
	| Phase  |
	| Phase1 |
	| Phase2 |
	| Phase3 |

Scenario: 365 Employment check not requested for Phase when learner stopped
	Given an apprenticeship incentive has been submitted in phase '<Phase>'
	And start of apprenticeship employment checks have passed
	And the learner data identifies the learner as not in learning anymore
	And 3 weeks has elapsed since 365 days after the due date of the second payment
	When an ILR submission is received for that learner
	Then the incentive is updated to stopped
	And a re-request 365 employment check is not requested for the phase '<Phase>'
Examples:
	| Phase  |
	| Phase1 |
	| Phase2 |
	| Phase3 |

Scenario: 365 Employment check not requested when payment has been made
	Given an apprenticeship incentive has been submitted in phase '<Phase>'
	And start of apprenticeship employment checks have passed
	And 3 weeks has elapsed since 365 days after the due date of the second payment which was previously paid
	When an ILR submission is received for that learner
	Then a 365 employment check is not requested
Examples:
	| Phase  |
	| Phase1 |
	| Phase2 |
	| Phase3 |
