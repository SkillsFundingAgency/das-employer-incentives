@activeCalendarPeriod
Feature: RefreshLearnerData
	When the request to refresh learner data for an apprenticeship incentive is received
	Then the learner data is either created or updated

Scenario: Request to refresh learner data for a new Apprenticeship Incentive where no learner data exists
	Given an apprenticeship incentive exists and without a corresponding learner match record
	When the learner data is refreshed for the apprenticeship incentive
	Then the apprenticeship incentive learner data is created for the application without any submission data

Scenario: Request to refresh learner data for a new Apprenticeship Incentive where learner data exists
	Given an apprenticeship incentive exists and with a corresponding learner match record
	And the latest learner data has active in learning data
	When the learner data is refreshed for the apprenticeship incentive
	Then the apprenticeship incentive learner data is created for the application with submission data

Scenario: Request to refresh learner data for a new Apprenticeship Incentive where learner data previously refreshed
	Given an apprenticeship incentive exists and has previously been refreshed
	And the latest learner data has active in learning data
	When the learner data is refreshed for the apprenticeship incentive
	Then the apprenticeship incentive learner data is updated for the application with submission data

Scenario: Request to refresh learner data for a new Apprenticeship Incentive where learner has provider data lock
	Given an apprenticeship incentive exists
	And the latest learner data has a data locked price episode
	When the learner data is refreshed for the apprenticeship incentive
	And the locked price episode period matches the next pending payment period
	Then the apprenticeship incentive learner data is updated indicating data lock

Scenario: Request to refresh learner data for a new Apprenticeship Incentive where training found for a different apprenticeship
	Given an apprenticeship incentive exists and with a corresponding learner match record
	And the latest learner data has training entries for a different apprenticeship
	When the learner data is refreshed for the apprenticeship incentive
	Then the apprenticeship incentive learner data is updated indicating learning not found

Scenario: Request to refresh learner data for a new Apprenticeship Incentive where no learning aims were found
	Given an apprenticeship incentive exists and with a corresponding learner match record
	And the latest learner data has no training entries
	When the learner data is refreshed for the apprenticeship incentive
	Then the apprenticeship incentive learner data is updated indicating learning not found

Scenario: Request to refresh learner data for a new Apprenticeship Incentive where no ZPROG001 training were found
	Given an apprenticeship incentive exists and with a corresponding learner match record
	And the latest learner data has no ZPROG001 training entries
	When the learner data is refreshed for the apprenticeship incentive
	Then the apprenticeship incentive learner data is updated indicating learning not found

Scenario: Request to refresh learner data for a new Apprenticeship Incentive with a training episode with no end date
	Given an apprenticeship incentive exists and with a corresponding learner match record
	And the latest learner data has a matching training episode with no end date
	When the learner data is refreshed for the apprenticeship incentive
	Then the apprenticeship incentive learner data is updated with days in learning counted up until the census date