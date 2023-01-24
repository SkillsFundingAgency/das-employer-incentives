@activeCalendarPeriod
Feature: EmploymentCheckEI1991
Stopped Learners: Employment Checks to be inspired when 'Stop' and 'Resume' is recorded in the same reporting period

Scenario: First 365 plus 21 check - 'Stop' and 'Resume' in the same reporting period
	Given a learner has had a 365 plus 21 day first check
	When the stop date and resume is received in the same reporting period
	Then the employment checks are re-inspired
	And there is archiving of previous checks
