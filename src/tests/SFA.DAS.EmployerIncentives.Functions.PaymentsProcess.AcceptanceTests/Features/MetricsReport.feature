@activeCalendarPeriod
@azureBlobStorage
@messageBus
Feature: MetricsReport
	In order to make approve payments once they have been validated
	As a employer incentives service
	I want to create a metrics report to be sent for approval

Scenario: When payment processing requires metrics approval
	Given valid payments exist
	When the payment process is run and approved
	Then the Metrics report is generated and sent
	And the Metrics report emails are sent
	And a Slack message is posted to notify the Metrics report generation
	And the payments can be sent to Business Central

Scenario: When payment processing requires metrics approval which is not received
	Given valid payments exist
	When the payment process is run but approvals are not received in time
	Then the Metrics report is generated and sent
	And the Metrics report emails are sent
	And the payment run is rejected
	And the payments are not sent to Business Central
