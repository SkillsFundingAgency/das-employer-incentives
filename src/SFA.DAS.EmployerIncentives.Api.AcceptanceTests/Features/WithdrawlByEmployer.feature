@database
@api
@domainMessageHandlers
@messageBus
Feature: WithdrawlByEmployer
	In order to handle an Employer withdrawing an apprenticeship from the incentive scheme
	As the employer incentive sheme
	I want to enable the apprenticeship to be removed from the scheme to prevent payments from being made

Scenario: Withdrawl status set against an application
	Given an incentive application has been made without being submitted
	When the apprenticeship application is withdrawn from the scheme
	Then the incentive application status is updated to indicate the employer withdrawl

Scenario: Withdrawl status set against a ULN with multiple applications 
	Given multiple incentive applications have been made for the same ULN without being submitted
	When the apprenticeship application is withdrawn from the scheme
	Then each incentive application status is updated to indicate the employer withdrawl
	
Scenario: Employer withdrawl removes incentive after an application has been submitted
	Given an apprenticeship incentive with pending payments exists as a result of an incentive application
	When the apprenticeship application is withdrawn from the scheme
	Then the apprenticeship incentive and it's pending payments are removed from the system
