@activeCalendarPeriod
Feature: LearningStopped
	When the refreshed learner data contains a learning stopped change of circumstance
	Then the apprenticeship incentive is updated to a stopped state

Scenario: Learner data contains a learning stopped change
	Given an apprenticeship incentive exists
	And the learner data identifies the learner as not in learning anymore
	When the incentive learner data is refreshed
	Then the incentive is updated to stopped
	And the stopped change of circumstance is saved
	And the learner data stopped date is stored
	And the learner start break in learning is stored

Scenario: Apprenticeship has unpaid earnings after the stopped date
	Given an apprenticeship incentive exists
	And the learner data identifies the learner as not in learning anymore
	And the apprenticeship incentive has unpaid earnings after the stopped date
	When the incentive learner data is refreshed
	Then the incentive is updated to stopped
	And the existing pending payments are removed

Scenario: Apprenticeship has paid earnings after the stopped date
	Given an apprenticeship incentive exists
	And the learner data identifies the learner as not in learning anymore
	And the apprenticeship incentive has paid earnings after the stopped date
	When the incentive learner data is refreshed
	Then the incentive is updated to stopped
	And the existing paid pending payments are clawed back

Scenario: Learner data contains a learning resumed change
	Given an apprenticeship incentive exists that has stopped learning
	And the learner data identifies the learner as in learning
	When the incentive learner data is refreshed
	Then the incentive is updated to active
	And the resumed change of circumstance is saved
	And the learner data resumed date is stored
	And the learner resume break in learning is stored
	
Scenario: Learner data contains a learning resumed change with start date before the start of assumed break in learning (defect EI-1195)
	Given an apprenticeship incentive exists that has stopped learning
	And the learner data identifies the learner as in learning with start date before recorded break date
	When the incentive learner data is refreshed
	Then the incentive is updated to active
	And the most recent break in learning record is deleted
	And the learner data stopped and resumed dates are deleted

Scenario: Learning data contains a change to learning stopped date
	Given an apprenticeship incentive exists that has stopped learning before the first payment is due
	And the learner data identifies the learning stopped date has changed to a date after the first payment is due
	When the incentive learner data is refreshed
	Then the first payment is recalculated
	And the stopped change of circumstance is updated
	And the learner start break in learning for the change in stopped date is stored

Scenario: Learning data contains a change to learning stopped date with a clawed back payment
	Given an apprenticeship incentive exists that has stopped learning before the first payment is due
	And the learner data identifies the learning stopped date has changed to a date after the first payment is due
	And the first payment has previously been clawed back
	When the incentive learner data is refreshed
	Then the previously clawed back payment is recalculated
	And the stopped change of circumstance is updated
	And the learner start break in learning for the change in stopped date is stored

Scenario Outline: Retrospective stopped tests on stopped incentive
	Given a stopped apprenticeship incentive exists 	
	And the first payment is <firstPaymentIsPaid>
	And the second payment is <secondPaymentIsPaid>
	And the stop date is <whenfirstPaymentIsPaid> the first payment and <whenSecondPaymentIsPaid> the second payment
	And the learner data identifies the learning stopped date has changed
	When the incentive learner data is refreshed
	Then the incentive is updated to stopped
	And the first earning is <firstEarningResult>
	And the second earning is <secondEarningResult>	

Examples:
    | firstPaymentIsPaid | secondPaymentIsPaid | whenfirstPaymentIsPaid | whenSecondPaymentIsPaid | firstEarningResult | secondEarningResult |
    | not paid           | not paid            | before                 | before                  | deleted            | deleted             |
    | not paid           | not paid            | on                     | before                  | retained           | deleted             |
    | not paid           | not paid            | after                  | before                  | retained           | deleted             |
    | not paid           | not paid            | after                  | on                      | retained           | retained            |
    | not paid           | not paid            | after                  | after                   | retained           | retained            |
    | paid               | not paid            | before                 | before                  | clawedback         | deleted             |
    | paid               | not paid            | on                     | before                  | retained           | deleted             |
    | paid               | not paid            | after                  | before                  | retained           | deleted             |
    | paid               | not paid            | after                  | on                      | retained           | retained            |
    | paid               | not paid            | after                  | after                   | retained           | retained            |
    | paid               | paid                | before                 | before                  | clawedback         | clawedback          |
    | paid               | paid                | on                     | before                  | retained           | clawedback          |
    | paid               | paid                | after                  | before                  | retained           | clawedback          |
    | paid               | paid                | after                  | on                      | retained           | retained            |
    | paid               | paid                | after                  | after                   | retained           | retained            |