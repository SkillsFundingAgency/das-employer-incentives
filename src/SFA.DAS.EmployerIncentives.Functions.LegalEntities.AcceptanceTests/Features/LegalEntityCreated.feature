@database
@messageBus
Feature: LegalEntityCreated
	When a legal entity has been created
	I want it to be available to Employer Incentives

Scenario: A new legal entity is stored in Employer Inventives
	Given I have a legal entity that is not stored in Employer Inventives
	When the legal entity is added to an account
	Then the legal entity should be stored in Employer Inventives

Scenario: A legal entity is already stored in Employer Inventives
	Given I have a legal entity that is already stored in Employer Inventives
	When the legal entity is added to an account
	Then the legal entity should be stored in Employer Inventives

Scenario: An invalid legal entity is not stored in Employer Inventives
	Given I have a legal entity that is not valid in Employer Inventives
	When the legal entity is added to an account
	Then the legal entity should not be stored in Employer Inventives