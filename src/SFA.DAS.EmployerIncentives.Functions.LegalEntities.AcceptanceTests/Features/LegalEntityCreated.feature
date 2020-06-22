@database
@messageBus
Feature: LegalEntityCreated
	In order know when a legal entity has been created
	I want to be shown when its available

Scenario: A new legal entity has been created
	Given I have a legal entity that is not in the database
	When added legal entity event is triggered
	Then the legal entity should be available

Scenario: A new legal entity has already been created
	Given I have a legal entity that is already in the database
	When added legal entity event is triggered
	Then the legal entity should be available

Scenario: An invalid legal entity event has been added
	Given I have an invalid legal entity that is new
	When added legal entity event is triggered
	Then the legal entity should not be available