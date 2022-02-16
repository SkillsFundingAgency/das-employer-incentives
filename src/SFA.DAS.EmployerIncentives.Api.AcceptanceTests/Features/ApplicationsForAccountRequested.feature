@database
@api
@activeCalendarPeriod

Feature: ApplicationsForAccountRequested
	In order to manage incentive applications
	As an account holder
	I want to be able to retrieve a list of apprenticeships applied for from my account

Scenario: A list of apprenticeships is requested
	Given an account that is in employer incentives
	When a client requests the apprenticeships for the account
	Then the apprenticeships are returned

Scenario: No learner record
	Given an account that is in employer incentives
	When there is no learner record for an apprenticeship
	When a client requests the apprenticeships for the account
	Then the apprenticeship is returned with no learner match found

Scenario: Learner record with no learner match
	Given an account that is in employer incentives
	When there is a learner record with no learner match for an apprenticeship
	When a client requests the apprenticeships for the account
	Then the apprenticeship is returned with no learner match found

Scenario: Learner record with learner match
	Given an account that is in employer incentives
	When there is a learner record with a learner match for an apprenticeship
	When a client requests the apprenticeships for the account
	Then the apprenticeship is returned with learner match found

Scenario: Learner record with data lock
	Given an account that is in employer incentives
	When there is a learner record with a data lock for an apprenticeship
	When a client requests the apprenticeships for the account
	Then the apprenticeship is returned with data lock set to true

Scenario: Learner record with no learning found
	Given an account that is in employer incentives
	When there is a learner record with in learning set to false for an apprenticeship
	When a client requests the apprenticeships for the account
	Then the apprenticeship is returned with in learning set to false

Scenario: Learner record with paused payments
	Given an account that is in employer incentives
	When there is an incentive with payments paused for an apprenticeship
	When a client requests the apprenticeships for the account
	Then the apprenticeship is returned with payments paused set to true

Scenario: Payment sent
	Given an account that is in employer incentives
	When there is a '<Earning Type>' payment that has been sent
	When a client requests the apprenticeships for the account
	Then the apprenticeship is returned with the '<Earning Type>' payment date set to the paid date 
	And the '<Earning Type>' payment amount is set to the calculated payment amount	
Examples:
| Earning Type  |
| FirstPayment  |
| SecondPayment |

Scenario: Payment calculated but not sent
	Given an account that is in employer incentives
	When there is a '<Earning Type>' payment that has been calculated but not sent
	When a client requests the apprenticeships for the account
	Then the apprenticeship is returned with the '<Earning Type>' payment date set to the calculated date
	And the '<Earning Type>' payment amount is set to the calculated payment amount
	And the '<Earning Type>' payment estimated is set to True
Examples:
| Earning Type  |
| FirstPayment  |
| SecondPayment |

Scenario: Payment calculated but not generated
	Given an account that is in employer incentives
	When there is a '<Earning Type>' payment that has been calculated but not generated
	When a client requests the apprenticeships for the account
	Then the apprenticeship is returned with the '<Earning Type>' estimated payment date set to the following month
	And the '<Earning Type>' payment amount is set to the pending payment amount
	And the '<Earning Type>' payment estimated is set to True
Examples:
| Earning Type  |
| FirstPayment  |
| SecondPayment |

Scenario: Payment calculated but not generated in current active period
	Given an account that is in employer incentives
	When there is a '<Earning Type>' payment that has been calculated but not generated for the current period
	When a client requests the apprenticeships for the account
	Then the apprenticeship is returned with the '<Earning Type>' payment date set to the next active period
	And the '<Earning Type>' payment amount is set to the pending payment amount
	And the '<Earning Type>' payment estimated is set to True
Examples:
| Earning Type  |
| FirstPayment  |
| SecondPayment |

Scenario: New employment agreement signature required
	Given an account that is in employer incentives with a signed employer agreement version of '<Signed Agreement Version>'
	When there is an incentive with a minimum employer agreement version of '<Minimum Agreement Version>'
	When a client requests the apprenticeships for the account
	Then the apprenticeship is returned with requires new employment agreement set to '<New Agreement Required>'
Examples:
| Signed Agreement Version | Minimum Agreement Version | New Agreement Required |
| null                     | null                      | true                   |
| null                     | 4                         | true                   |
| 4	                       | null                      | false                  |
| 4						   | 4                         | false                  |
| 5						   | 6                         | true                   |
| 7						   | 6                         | false                  |

Scenario: Employment check status
	Given an account that is in employer incentives
	When there is an 'EmployedAtStartOfApprenticeship' payment validation status of '<Employed At Start Of Apprenticeship>'
	And there is an 'EmployedBeforeSchemeStarted' payment validation status of '<Employed Before Scheme Started>'
	And there is an 'EmployedAtStartOfApprenticeship' employment check result of '<First Employment Check Result>'
	And there is an 'EmployedBeforeSchemeStarted' employment check result of '<Second Employment Check Result>'
	When a client requests the apprenticeships for the account
	Then the apprenticeship is returned with employment check status of '<Employment Check Status>'
Examples:
| Employed At Start Of Apprenticeship	| Employed Before Scheme Started | First Employment Check Result | Second Employment Check Result | Employment Check Status |
| true									| true							 | true                          | false                          | true                    |
| true									| false							 | true                          | true                           | false                   |
| false									| true							 | false                         | false                          | false                   |
| false									| false							 | false                         | true                           | false                   |
