﻿@database
@api
@domainMessageHandlers
@messageBus
Feature: WithdrawalByEmployer
	In order to handle an Employer withdrawing an apprenticeship from the incentive scheme
	As the employer incentive sheme
	I want to enable the apprenticeship to be removed from the scheme to prevent payments from being made

Scenario: Withdrawal status set against an apprenticeship in an application
	Given an incentive application has been made without being submitted
	When the apprenticeship application is withdrawn from the scheme
	Then the incentive application status is updated to indicate the employer withdrawal

Scenario: Withdrawal status set against a ULN with multiple apprenticeships in applications 
	Given multiple incentive applications have been made for the same ULN without being submitted
	When the apprenticeship application is withdrawn from the scheme
	Then each incentive application status is updated to indicate the employer withdrawal
	
Scenario: Employer withdrawal removes incentive after an apprenticeship application has been submitted
	Given an apprenticeship incentive with pending payments exists as a result of an incentive application
	When the apprenticeship application is withdrawn from the scheme
	Then the apprenticeship incentive and it's pending payments are removed from the system

Scenario: cannot withdraw an application that has been submitted and the incentives has had payments
	Given an incentive application has been made, submitted and has payments	
	When the apprenticeship application is withdrawn from the scheme
	Then an error is returned
