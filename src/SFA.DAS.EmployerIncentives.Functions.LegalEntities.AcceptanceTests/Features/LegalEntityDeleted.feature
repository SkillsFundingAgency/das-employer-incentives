@database
@messageBus
Feature: LegalEntityDeleted
	When a legal entity has been removed from an account
	Then is is no longer available in Employer Incentives

Scenario: A legal entity has been removed from an account
	Given a legal entity that is in employer incentives
	When a legal entity is removed from an account
	Then the legal entity should no longer be available in employer incentives

Scenario: A legal entity that has is not available  in employer incentives
	Given a legal entity is not available in employer incentives
	When a legal entity is removed from an account
	Then the legal entity is still not available in employer incentives
