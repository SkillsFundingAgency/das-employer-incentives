@database
@api
Feature: LegalEntityCreated
	When a legal entity has been added to an account
	Then is is available in Employer Incentives

Scenario: A legal entity is added to an account
	Given the legal entity is in not available in Employer Incentives
	When the legal entity is added to an account
	Then the legal entity should be available in Employer Incentives

Scenario: A legal entity associated to an account is already available in Employer Incentives
	Given the legal entity is already available in Employer Incentives
	When the legal entity is added to an account
	Then the legal entity should still be available in Employer Incentives