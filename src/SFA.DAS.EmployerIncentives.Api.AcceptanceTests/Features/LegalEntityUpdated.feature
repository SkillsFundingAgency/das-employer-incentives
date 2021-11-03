@database
@api
Feature: LegalEntityUpdated
	When an legal entity has been updated in an account
	Then the latest details are available in Employer Incentives

Scenario: A legal entity is amended in an account
	Given the legal entity is already available in Employer Incentives
	When the legal entity name is amended in an account
	Then the legal entity should be updated with the latest name
