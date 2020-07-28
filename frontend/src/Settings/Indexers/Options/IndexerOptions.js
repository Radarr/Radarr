import PropTypes from 'prop-types';
import React from 'react';
import FieldSet from 'Components/FieldSet';
import Form from 'Components/Form/Form';
import FormGroup from 'Components/Form/FormGroup';
import FormInputGroup from 'Components/Form/FormInputGroup';
import FormLabel from 'Components/Form/FormLabel';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import { inputTypes } from 'Helpers/Props';
import translate from 'Utilities/String/translate';

function IndexerOptions(props) {
  const {
    advancedSettings,
    isFetching,
    error,
    settings,
    hasSettings,
    onInputChange
  } = props;

  return (
    <FieldSet legend={translate('Options')}>
      {
        isFetching &&
          <LoadingIndicator />
      }

      {
        !isFetching && error &&
          <div>Unable to load indexer options</div>
      }

      {
        hasSettings && !isFetching && !error &&
          <Form>
            <FormGroup>
              <FormLabel>Minimum Age</FormLabel>

              <FormInputGroup
                type={inputTypes.NUMBER}
                name="minimumAge"
                min={0}
                unit="minutes"
                helpText="Usenet only: Minimum age in minutes of NZBs before they are grabbed. Use this to give new releases time to propagate to your usenet provider."
                onChange={onInputChange}
                {...settings.minimumAge}
              />
            </FormGroup>

            <FormGroup>
              <FormLabel>Retention</FormLabel>

              <FormInputGroup
                type={inputTypes.NUMBER}
                name="retention"
                min={0}
                unit="days"
                helpText="Usenet only: Set to zero to set for unlimited retention"
                onChange={onInputChange}
                {...settings.retention}
              />
            </FormGroup>

            <FormGroup>
              <FormLabel>Maximum Size</FormLabel>

              <FormInputGroup
                type={inputTypes.NUMBER}
                name="maximumSize"
                min={0}
                unit="MB"
                helpText="Maximum size for a release to be grabbed in MB. Set to zero to set to unlimited"
                onChange={onInputChange}
                {...settings.maximumSize}
              />
            </FormGroup>

            <FormGroup>
              <FormLabel>Prefer Indexer Flags</FormLabel>

              <FormInputGroup
                type={inputTypes.CHECK}
                name="preferIndexerFlags"
                helpText="Prioritize releases with special flags"
                helpLink="https://github.com/Radarr/Radarr/wiki/Indexer-Flags"
                onChange={onInputChange}
                {...settings.preferIndexerFlags}
              />
            </FormGroup>

            <FormGroup>
              <FormLabel>Availability Delay</FormLabel>

              <FormInputGroup
                type={inputTypes.NUMBER}
                name="availabilityDelay"
                unit="Days"
                helpText="Amount of time before or after available date to search for Movie"
                onChange={onInputChange}
                {...settings.availabilityDelay}
              />
            </FormGroup>

            <FormGroup
              advancedSettings={advancedSettings}
              isAdvanced={true}
            >
              <FormLabel>RSS Sync Interval</FormLabel>

              <FormInputGroup
                type={inputTypes.NUMBER}
                name="rssSyncInterval"
                min={0}
                max={120}
                unit="minutes"
                helpText="Interval in minutes. Set to zero to disable (this will stop all automatic release grabbing)"
                helpTextWarning="This will apply to all indexers, please follow the rules set forth by them"
                helpLink="https://github.com/Radarr/Radarr/wiki/RSS-Sync"
                onChange={onInputChange}
                {...settings.rssSyncInterval}
              />
            </FormGroup>

            <FormGroup
              advancedSettings={advancedSettings}
              isAdvanced={true}
            >
              <FormLabel>Whitelisted Subtitle Tags</FormLabel>

              <FormInputGroup
                type={inputTypes.TEXT_TAG}
                name="whitelistedHardcodedSubs"
                helpText="Subtitle tags set here will not be considered hardcoded"
                onChange={onInputChange}
                {...settings.whitelistedHardcodedSubs}
              />
            </FormGroup>

            <FormGroup
              advancedSettings={advancedSettings}
              isAdvanced={true}
            >
              <FormLabel>Allow Hardcoded Subs</FormLabel>

              <FormInputGroup
                type={inputTypes.CHECK}
                name="allowHardcodedSubs"
                helpText="Detected hardcoded subs will be automatically downloaded"
                onChange={onInputChange}
                {...settings.allowHardcodedSubs}
              />
            </FormGroup>
          </Form>
      }
    </FieldSet>
  );
}

IndexerOptions.propTypes = {
  advancedSettings: PropTypes.bool.isRequired,
  isFetching: PropTypes.bool.isRequired,
  error: PropTypes.object,
  settings: PropTypes.object.isRequired,
  hasSettings: PropTypes.bool.isRequired,
  onInputChange: PropTypes.func.isRequired
};

export default IndexerOptions;
