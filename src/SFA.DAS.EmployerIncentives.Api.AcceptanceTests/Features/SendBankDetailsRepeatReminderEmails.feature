@database
@messageBus
@api
@activeCalendarPeriod
Feature: SendBankDetailsRepeatReminderEmails
	In order to receive incentive payments
	As an employer
	I need to be reminded to supply my bank details to receive payment

Scenario: Employer has applied for the incentive payment but not supplied bank details
When an employer has submitted an application after the cut off date and not supplied bank details
And the check for accounts without bank details is triggered
Then the employer is sent a reminder email to supply their bank details in order to receive payment
