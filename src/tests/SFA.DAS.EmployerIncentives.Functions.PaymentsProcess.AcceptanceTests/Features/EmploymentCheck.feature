@activeCalendarPeriod
Feature: EmploymentCheck

Scenario: Employment checks requested on first ILR submission (phase 1)
	Given an apprenticeship incentive has been submitted in phase 1
	And we have not previously requested an employment check for the learner
	And 6 weeks has elapsed since the start date of the apprenticeship
	When an ILR submission is received for that learner
	Then a new employment check is requested to ensure the apprentice was not employed in the 6 months prior to phase 1 starting
	And a new employment check is requested to ensure the apprentice was employed in the six weeks following their start date