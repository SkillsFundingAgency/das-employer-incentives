@database
@api
@accountApi

Feature: GetEmployerVendorIdForLegalEntity
	In order to ensure that the end to end payment process works effectively
	As a service owner
	I want to check whether an account legal entity has an associated vendor id

Scenario: Account legal entity has a vendor id assigned
Given a legal entity exists within an account
And the legal entity has a vendor id assigned
Then the vendor id associated with the account legal entity is returned

Scenario: Account legal entity does not have a vendor id assigned
Given a legal entity exists within an account
And the legal entity does not have a vendor id assigned
Then no vendor id is returned