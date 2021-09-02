@activeCalendarPeriod
Feature: RefreshLearnerDataDuringAcademicYearRollover
	Simple calculator for adding two numbers

Scenario: Learning stops in R1 after learner has been added for R13
	Given a successful learner match response for an incentive application
	When the most recent price episode has no periods
	And the previous price episode has a period with a matching apprenticeship ID
	Then trigger a learning stopped Change of Circumstance
	And record the learning stopped date as the day after the last price episode end date
