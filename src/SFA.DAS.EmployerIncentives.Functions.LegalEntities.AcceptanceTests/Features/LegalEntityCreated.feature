@database
@messageBus
Feature: LegalEntityCreated
	When a legal entity has been added to an account
	Then is is available in Employer Incentives

Scenario: A legal entity is added to an account
	Given the legal entity is not stored in Employer Incentives
	When the legal entity is added to an account
	Then the legal entity should be stored in Employer Incentives

Scenario: A legal entity associated to an account and already stored in Employer Incentives is added to an account
	Given the legal entity is already stored in Employer Incentives
	When the legal entity is added to an account
	Then the legal entity should be stored in Employer Incentives

Scenario: A legal entity that is not valid in Employer Incentives is added to an account
	Given the legal entity is not valid for Employer Incentives
	When the legal entity is added to an account
	Then the legal entity should not be stored in Employer Incentives