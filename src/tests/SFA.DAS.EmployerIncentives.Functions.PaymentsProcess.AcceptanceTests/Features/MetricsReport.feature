@activeCalendarPeriod
@azureBlobStorage
Feature: MetricsReport
	In order to make approve payments once they have been validated
	As a employer incentives service
	I want to create a metrics report to be sent for approval

Scenario: When payment processing requires metrics approval
	Given valid payments exist
	When the payment process is run
	Then the Metrics report is generated and sent
