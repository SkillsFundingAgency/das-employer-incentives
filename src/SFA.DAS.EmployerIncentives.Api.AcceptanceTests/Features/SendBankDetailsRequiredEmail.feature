@database
@api
Feature: SendBankDetailsRequiredEmail
	In order to complete the application for an incentive payment
	As an employer
	I need to be reminded to supply my bank details to receive payment

Scenario: Employer cannot supply bank details immediately
When a bank details required email is sent for a valid account, legal entity and email address
Then the employer is sent a reminder email to supply their bank details

Scenario: Email address invalid
When a bank details required email is sent with an invalid email address
Then the email is not set and an error response returned

Scenario: Account id invalid
When A bank details required email is sent with an invalid account id
Then the email is not set and an error response returned

Scenario: Account legal entity id invalid
When a bank details required email is sent with an invalid account legal entity id 
Then the email is not set and an error response returned

Scenario: Account bank details url invalid
When a bank details required email is sent with an invalid account bank details url
Then the email is not set and an error response returned