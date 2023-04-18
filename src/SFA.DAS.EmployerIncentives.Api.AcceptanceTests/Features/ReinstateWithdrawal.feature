#@database
#@api
#@domainMessageHandlers
#@messageBus
#@activeCalendarPeriod
#Feature: ReinstateWithdrawal
#
#Scenario: Withdrawal status reset against an apprenticeship in an application
#	Given an incentive application has been withdrawn by compliance
#	When a request is made to reinstate the application
#	Then the incentive application status is reset
#	And the apprenticeship incentive status is reset to Paused
#	And the earnings are recalculated