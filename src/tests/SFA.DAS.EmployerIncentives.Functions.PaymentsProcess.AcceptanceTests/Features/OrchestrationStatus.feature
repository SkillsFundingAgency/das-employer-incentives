Feature: OrchestrationStatus
	Simple calculator for adding two numbers

@activeCalendarPeriod
Scenario: Add two numbers
	Given an orchestrator has been invoked
	When orchestrators check all status function is run
	Then the status results are shown