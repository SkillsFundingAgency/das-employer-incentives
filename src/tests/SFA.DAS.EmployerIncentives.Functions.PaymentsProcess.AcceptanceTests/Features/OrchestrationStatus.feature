Feature: OrchestrationStatus
	Simple calculator for adding two numbers

@activeCalendarPeriod
Scenario: Can log Orchestration Status
	Given an orchestrator has been invoked
	When orchestrators check all status function is run
	Then the status results are shown