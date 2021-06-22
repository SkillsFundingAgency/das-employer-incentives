@activeCalendarPeriod
Feature: LearnerMatchFailed
	When the Learner Match process fails for a given learner
	Then the the fact of failure is recorded and the Learner Match process is not stopped

Scenario: Learner Match fails for one of the learners
	Given existing apprenticeship incentives
	And the learner match process has been triggered
	When an exception occurs for a learner
	Then a record of learner match failure is created for the learner
	And the learner match process is continued for all remaining learners
	And a record of learner match success is created for all remaining learners