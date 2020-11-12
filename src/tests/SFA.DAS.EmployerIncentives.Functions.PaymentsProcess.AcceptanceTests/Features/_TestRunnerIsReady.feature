Feature: TestRunnerIsReady
	In order to run acceptance tests
	As a developer
	I want to ensure test runner is setup and ready

Scenario: Test Runner Is Ready
	Given tests are setup
	When a database instance
	And a learner match api
	And a functions host
	Then test runner is ready