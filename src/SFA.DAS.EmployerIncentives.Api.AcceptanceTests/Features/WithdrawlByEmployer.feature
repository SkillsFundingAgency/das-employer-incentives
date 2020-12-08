@database
@api
Feature: WithdrawlByEmployer
	In order to handle an Employer withdrawing an apprenticeship from the incentive scheme
	As the employer incentive sheme
	I want to enable the apprenticeship to be removed from the scheme to prevent payments from being made

Scenario: Withdrawl status set against an application
	Given an incentive application has been made without being submitted
	When the apprenticeship application is withdrawn from the scheme
	Then the incentive application status is updated to indicate the employer withdrawl