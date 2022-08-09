@activeCalendarPeriod
Feature: EmploymentCheck

Scenario: Employment checks requested on first ILR submission (phase 1)
	Given an apprenticeship incentive has been submitted in phase 1
	And we have not previously requested an employment check for the learner
	And 6 weeks has elapsed since the start date of the apprenticeship
	When an ILR submission is received for that learner
	Then a new employment check is requested to ensure the apprentice was not employed in the 6 months prior to phase 1 starting
	And a new employment check is requested to ensure the apprentice was employed in the six weeks following their start date
	And a 365 employment check is not requested

Scenario: Employment checks requested on first ILR submission (phase 2)
	Given an apprenticeship incentive has been submitted in phase 2
	And we have not previously requested an employment check for the learner
	And 6 weeks has elapsed since the start date of the apprenticeship
	When an ILR submission is received for that learner
	Then a new employment check is requested to ensure the apprentice was not employed in the 6 months prior to phase 2 starting
	And a new employment check is requested to ensure the apprentice was employed in the six weeks following their start date
	And a 365 employment check is not requested

Scenario: Employment checks requested on first ILR submission (phase 3)
	Given an apprenticeship incentive has been submitted in phase 3
	And we have not previously requested an employment check for the learner
	And 6 weeks has elapsed since the start date of the apprenticeship
	When an ILR submission is received for that learner
	Then a new employment check is requested to ensure the apprentice was not employed in the 6 months prior to phase 3 starting
	And a new employment check is requested to ensure the apprentice was employed in the six weeks following their start date
	And a 365 employment check is not requested

Scenario: Employment checks requested on Start date change of circumstance (phase 1)
	Given an apprenticeship incentive has been submitted in phase 1
	And we have previously requested an employment check for the learner
	And the learner data is updated with new valid start date for phase 1
	And 6 weeks has elapsed since the start date of the apprenticeship
	When an ILR submission is received for that learner
	Then a new employment check is requested to ensure the apprentice was not employed in the 6 months prior to phase 1 starting
	And a new employment check is requested to ensure the apprentice was employed in the six weeks following their start date
	And a 365 employment check is not requested

Scenario: Employment checks requested on Start date change of circumstance (phase 2)
	Given an apprenticeship incentive has been submitted in phase 2
	And we have previously requested an employment check for the learner
	And the learner data is updated with new valid start date for phase 2
	And 6 weeks has elapsed since the start date of the apprenticeship
	When an ILR submission is received for that learner
	Then a new employment check is requested to ensure the apprentice was not employed in the 6 months prior to phase 2 starting
	And a new employment check is requested to ensure the apprentice was employed in the six weeks following their start date
	And a 365 employment check is not requested

Scenario: Employment checks requested on Start date change of circumstance (phase 3)
	Given an apprenticeship incentive has been submitted in phase 3
	And we have previously requested an employment check for the learner
	And the learner data is updated with new valid start date for phase 3
	And 6 weeks has elapsed since the start date of the apprenticeship
	When an ILR submission is received for that learner
	Then a new employment check is requested to ensure the apprentice was not employed in the 6 months prior to phase 3 starting
	And a new employment check is requested to ensure the apprentice was employed in the six weeks following their start date
	And a 365 employment check is not requested

Scenario: 365 Employment check requested for Phase 1
	Given an apprenticeship incentive has been submitted in phase 1
	And start of apprenticeship employment checks have passed
	And 3 weeks has elapsed since 365 days after the due date of the second payment
	When an ILR submission is received for that learner
	Then a new 365 employment check is requested to ensure the apprentice was employed when the second payment was due for the phase 1

Scenario: 365 Employment check requested for Phase 2
	Given an apprenticeship incentive has been submitted in phase 2
	And start of apprenticeship employment checks have passed
	And 3 weeks has elapsed since 365 days after the due date of the second payment
	When an ILR submission is received for that learner
	Then a new 365 employment check is requested to ensure the apprentice was employed when the second payment was due for the phase 2

Scenario: 365 Employment check requested for Phase 3
	Given an apprenticeship incentive has been submitted in phase 3
	And start of apprenticeship employment checks have passed
	And 3 weeks has elapsed since 365 days after the due date of the second payment
	When an ILR submission is received for that learner
	Then a new 365 employment check is requested to ensure the apprentice was employed when the second payment was due for the phase 3

Scenario: 365 Employment check not re-requested for Phase 1 after initial failure
	Given an apprenticeship incentive has been submitted in phase 1	
	And start of apprenticeship employment checks have passed
	And 3 weeks has elapsed since 365 days after the due date of the second payment
	And the initial 365 check has failed
	When an ILR submission is received for that learner
	Then a re-request 365 employment check is not requested for the phase 1

Scenario: 365 Employment check not re-requested for Phase 2 after initial failure
	Given an apprenticeship incentive has been submitted in phase 2
	And start of apprenticeship employment checks have passed	
	And 3 weeks has elapsed since 365 days after the due date of the second payment
	And the initial 365 check has failed
	When an ILR submission is received for that learner
	Then a re-request 365 employment check is not requested for the phase 2

Scenario: 365 Employment check not re-requested for Phase 3 after initial failure
	Given an apprenticeship incentive has been submitted in phase 3
	And start of apprenticeship employment checks have passed	
	And 3 weeks has elapsed since 365 days after the due date of the second payment
	And the initial 365 check has failed
	When an ILR submission is received for that learner
	Then a re-request 365 employment check is not requested for the phase 3

Scenario: 365 Employment check re-requested for Phase 1 after initial failure
	Given an apprenticeship incentive has been submitted in phase 1
	And start of apprenticeship employment checks have passed
	And 6 weeks has elapsed since 365 days after the due date of the second payment
	And the initial 365 check has failed
	When an ILR submission is received for that learner
	Then a re-request 365 employment check is requested for the phase 1

Scenario: 365 Employment check re-requested for Phase 2 after initial failure
	Given an apprenticeship incentive has been submitted in phase 2
	And start of apprenticeship employment checks have passed
	And 6 weeks has elapsed since 365 days after the due date of the second payment
	And the initial 365 check has failed
	When an ILR submission is received for that learner
	Then a re-request 365 employment check is requested for the phase 2

Scenario: 365 Employment check re-requested for Phase 3 after initial failure
	Given an apprenticeship incentive has been submitted in phase 3
	And start of apprenticeship employment checks have passed
	And 6 weeks has elapsed since 365 days after the due date of the second payment
	And the initial 365 check has failed
	When an ILR submission is received for that learner
	Then a re-request 365 employment check is requested for the phase 3

