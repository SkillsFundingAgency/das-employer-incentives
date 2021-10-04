@activeCalendarPeriod
Feature: LearnerMatchFailed
	When the Learner Match process fails for a given learner
	Then the the fact of failure is recorded and the Learner Match process is not stopped

Scenario: Learner Match fails for one of the learners
	Given existing apprenticeship incentives
	And an exception occurs for a learner
	When the learner match process has been triggered 
	Then a record of learner match failure is created for the learner
	And the learner match process is continued for all remaining learners
	And a record of learner match success is created for all remaining learners

Scenario: Learner Match returns invalid JSON response
	Given existing apprenticeship incentives
	And an invalid JSON response is returned for a learner
	When the learner match process has been triggered
	Then the JSON response is recorded for the failed learner match