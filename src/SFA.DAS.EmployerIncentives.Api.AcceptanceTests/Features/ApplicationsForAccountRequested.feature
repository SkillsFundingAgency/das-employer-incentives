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
	And a client requests the apprenticeships for the account
	Then the apprenticeship is returned with no learner match found

Scenario: Learner record with no learner match
	Given an account that is in employer incentives
	When there is a learner record with no learner match for an apprenticeship
	And a client requests the apprenticeships for the account
	Then the apprenticeship is returned with no learner match found

Scenario: Learner record with learner match
	Given an account that is in employer incentives
	When there is a learner record with a learner match for an apprenticeship
	And a client requests the apprenticeships for the account
	Then the apprenticeship is returned with learner match found

Scenario: Learner record with data lock
	Given an account that is in employer incentives
	When there is a learner record with a data lock for an apprenticeship
	And a client requests the apprenticeships for the account
	Then the apprenticeship is returned with data lock set to true

Scenario Outline: Learner record with data lock and validation override
	Given an account that is in employer incentives
	And there is a learner record with a data lock of '<HasDataLock>' for an apprenticeship
	And there is a data lock validation override and expiry of '<HasExpired>'
	When a client requests the apprenticeships for the account
	Then the apprenticeship is returned with data lock set to '<Expectation>'
Examples:
| HasDataLock | HasExpired | Expectation |
| true        | false      | false       |
| false       | false      | false       |
| true        | true       | true        |
| false       | true       | false       |

Scenario: Learner record with no learning found
	Given an account that is in employer incentives
	When there is a learner record with in learning set to false for an apprenticeship
	And a client requests the apprenticeships for the account
	Then the apprenticeship is returned with in learning set to false

Scenario Outline: Learner record with no learning found and validation override
	Given an account that is in employer incentives
	And there is a learner record with in learning set to '<InLearning>' for an apprenticeship
	And there is an IsInLearning validation override and expiry of '<HasExpired>'
	When a client requests the apprenticeships for the account
	Then the apprenticeship is returned with in learning set to '<Expectation>'
Examples:
| InLearning | HasExpired | Expectation |
| false      | false      | true        |
| true       | false      | true        |
| false      | true       | false       |
| true       | true       | true        |

Scenario: Learner record with paused payments
	Given an account that is in employer incentives
	When there is an incentive with payments paused for an apprenticeship
	And a client requests the apprenticeships for the account
	Then the apprenticeship is returned with payments paused set to true

Scenario: Payment sent
	Given an account that is in employer incentives
	When there is a '<Earning Type>' payment that has been sent
	And a client requests the apprenticeships for the account
	Then the apprenticeship is returned with the '<Earning Type>' payment date set to the paid date 
	And the '<Earning Type>' payment amount is set to the calculated payment amount	
Examples:
| Earning Type  |
| FirstPayment  |
| SecondPayment |

Scenario: Payment calculated but not sent
	Given an account that is in employer incentives
	When there is a '<Earning Type>' payment that has been calculated but not sent
	And a client requests the apprenticeships for the account
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
	And a client requests the apprenticeships for the account
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
	And a client requests the apprenticeships for the account
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
	And a client requests the apprenticeships for the account
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

Scenario Outline: EmployedAtStartOfApprenticeship Employment check status with override
	Given an account that is in employer incentives	
	When there is a validation override result of '<Validation Override Result>' and an 'EmployedAtStartOfApprenticeship' payment validation status of '<Employed At Start Of Apprenticeship>'
	And there is an 'EmployedBeforeSchemeStarted' payment validation status of '<Employed Before Scheme Started>'
	And there is an 'EmployedAtStartOfApprenticeship' employment check result of '<First Employment Check Result>'
	And there is an 'EmployedBeforeSchemeStarted' employment check result of '<Second Employment Check Result>'	
	When a client requests the apprenticeships for the account
	Then the apprenticeship is returned with employment check status of '<Employment Check Status>'
Examples:
| Employed At Start Of Apprenticeship | Employed Before Scheme Started | First Employment Check Result | Second Employment Check Result | Validation Override Result | Employment Check Status |
| true                                | true                           | false                         | true                           | true						 | true                    |
| false                               | true                           | false                         | true                           | true						 | true                    |
| true                                | true                           | true                          | true                           | true						 | true                    |
| false                               | true                           | true                          | true                           | true						 | true                    |
| true                                | true                           | false                         | true                           | false						 | true                    |
| false                               | true                           | false                         | true                           | false						 | false                   |
| true                                | true                           | true                          | true                           | false						 | true                    |
| false                               | true                           | true                          | true                           | false						 | false                   |

Scenario Outline: EmployedBeforeSchemeStarted Employment check status with override
	Given an account that is in employer incentives
	When there is an 'EmployedAtStartOfApprenticeship' payment validation status of '<Employed At Start Of Apprenticeship>'
	And there is a validation override result of '<Validation Override Result>' and an 'EmployedBeforeSchemeStarted' payment validation status of '<Employed Before Scheme Started>'
	And there is an 'EmployedAtStartOfApprenticeship' employment check result of '<First Employment Check Result>'
	And there is an 'EmployedBeforeSchemeStarted' employment check result of '<Second Employment Check Result>'	
	When a client requests the apprenticeships for the account
	Then the apprenticeship is returned with employment check status of '<Employment Check Status>'
Examples:
| Employed At Start Of Apprenticeship | Employed Before Scheme Started | First Employment Check Result | Second Employment Check Result | Validation Override Result | Employment Check Status |
| true                                | true                           | true                          | false                          | true						 | true                    |
| true                                | false                          | true                          | false                          | true						 | true                    |
| true                                | true                           | true                          | true                           | true						 | true                    |
| true                                | false                          | true                          | true                           | true						 | true                    |
| true                                | true                           | true                          | false                          | false						 | true                    |
| true                                | false                          | true                          | false                          | true						 | true                    |
| true                                | false                          | true                          | false                          | false						 | false                   |
| true                                | false                          | true                          | true                           | false						 | false                   |

Scenario: Multiple employment check payment validation statuses
	Given an account that is in employer incentives
	When there are failed employment check payment validations for the apprenticeship
	And new employment check results have been recorded
	And new employment check payment validations are recorded
	When a client requests the apprenticeships for the account
	Then the most recent employment check payment validation results are reflected in the payment statuses

Scenario: Payment validation results do not include employment check
	Given an account that is in employer incentives
	When new employment check results have been recorded
	And there are no payment validations for the apprenticeship
	When a client requests the apprenticeships for the account
	Then the employment check payment statuses are not set

Scenario: Employment check has no payment validation results
	Given an account that is in employer incentives
	When new employment check results have been recorded
	And there are no employment check payment validations for the apprenticeship
	When a client requests the apprenticeships for the account
	Then the employment check payment statuses are not set

Scenario: Employment check payment validation fails due to no employment check record
	Given an account that is in employer incentives
	When there are no employment check results for the apprenticeship
	And there are failed employment check payment validations for the apprenticeship
	When a client requests the apprenticeships for the account
	Then the employment check payment statuses are not set

Scenario: Employment check payment validation fails due to null employment check records
	Given an account that is in employer incentives
	When there are employment check results for the apprenticeship with null values
	And there are failed employment check payment validations for the apprenticeship
	When a client requests the apprenticeships for the account
	Then the employment check payment statuses are not set

Scenario: Payment stopped
	Given an account that is in employer incentives
	When the incentive has a status of '<Incentive Status>'
	And a client requests the apprenticeships for the account
	Then the payment statuses reflect the stopped status of '<Payment Stopped Status>'
Examples:
| Incentive Status	  | Payment Stopped Status |
| Stopped             | true                   |
| Active              | false                  |

Scenario: Payment clawed back
	Given an account that is in employer incentives
	When the '<Earning Type>' payment has been clawed back
	And a client requests the apprenticeships for the account
	Then the '<Earning Type>' clawback status reflects the amount clawed back and date
Examples:
| Earning Type  |
| FirstPayment  |
| SecondPayment |

Scenario: Application withdrawn
	Given an account that is in employer incentives
	When the application has been withdrawn by '<Withdrawn By>'
	And a client requests the apprenticeships for the account
	Then the payment statuses reflect that the application withdrawal was requested by '<Withdrawn By>'
Examples:
| Withdrawn By  |
| Employer		|
| Compliance	|

Scenario: Employment check status with 365 day check
	Given an account that is in employer incentives
	When there is an 'EmployedAtStartOfApprenticeship' payment validation status of 'true'
	And there is an 'EmployedBeforeSchemeStarted' payment validation status of 'true'
	And there is an 'EmployedAtStartOfApprenticeship' employment check result of 'true'
	And there is an 'EmployedBeforeSchemeStarted' employment check result of 'false'
	And there is an 'EmployedAt365Days' second payment validation status of '<Employed At 365 Days>'
	And there is an 'EmployedAt365PaymentDueDateFirstCheck' employment check result of '<365 Days Employment First Check Result>'
	And there is an 'EmployedAt365PaymentDueDateSecondCheck' employment check result of '<365 Days Employment Second Check Result>'
	When a client requests the apprenticeships for the account
	Then the apprenticeship is returned with second payment employment check status of '<Employment Check Status>'
Examples:
| Employed At 365 Days | 365 Days Employment First Check Result | 365 Days Employment Second Check Result | Employment Check Status |
| true                 | true                                   | null                                    | true                    |
| false                | false                                  | null                                    | null                    |
| false                | false                                  | false                                   | false                   |
| true                 | false                                  | true                                    | true                    |
| null                 | null                                   | null                                    | null                    |

Scenario Outline: EmployedAt365Days Employment check status with override
	Given an account that is in employer incentives
	When there is an 'EmployedAtStartOfApprenticeship' payment validation status of 'true'
	And there is an 'EmployedBeforeSchemeStarted' payment validation status of 'true'
	And there is an 'EmployedAtStartOfApprenticeship' employment check result of 'true'
	And there is an 'EmployedBeforeSchemeStarted' employment check result of 'false'
	When there is an override result of '<Employed At 365 Days Override>' and an 'EmployedAt365Days' second payment validation status of '<Employed At 365 Days>'	
	And there is an 'EmployedAt365PaymentDueDateFirstCheck' employment check result of 'false'
	And there is an 'EmployedAt365PaymentDueDateSecondCheck' employment check result of '<Employed At 365 Days>'
	And there is a validation override for validation step 'EmployedAt365Days' and expiry of '<HasExpired>'
	When a client requests the apprenticeships for the account
	Then the apprenticeship is returned with second payment employment check status of '<Employment Check Status>'
Examples:
| Employed At 365 Days | Employed At 365 Days Override | HasExpired | Employment Check Status |
| true                 | true                          | false      | true                    |
| false                | true                          | false      | true                    |
| true                 | true                          | true       | true                    |
| false                | true                          | true       | true                    |
| false                | false                         | true       | false                   |

Scenario: Learner record with stopped date before the second payment is paid
	Given an account that is in employer incentives
	When there is an incentive with a learning stopped date before the second payment is paid
	And a client requests the apprenticeships for the account
	Then the apprenticeship is returned with incentive completed set to 'False'

Scenario: Learner record with stopped date after the second payment is paid
	Given an account that is in employer incentives
	When there is an incentive with a learning stopped date after the second payment is paid
	And a client requests the apprenticeships for the account
	Then the apprenticeship is returned with incentive completed set to 'True'

Scenario: Learner record with stopped date and the second payment is not paid
	Given an account that is in employer incentives
	When there is an incentive with a learning stopped date and the second payment is not paid
	And a client requests the apprenticeships for the account
	Then the apprenticeship is returned with incentive completed set to 'False'