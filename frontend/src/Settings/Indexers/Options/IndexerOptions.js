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
          <div>
            {translate('UnableToLoadIndexerOptions')}
          </div>
      }

      {
        hasSettings && !isFetching && !error &&
          <Form>
            <FormGroup>
              <FormLabel>{translate('MinimumAge')}</FormLabel>

              <FormInputGroup
                type={inputTypes.NUMBER}
                name="minimumAge"
                min={0}
                unit="minutes"
                helpText={translate('MinimumAgeHelpText')}
                onChange={onInputChange}
                {...settings.minimumAge}
              />
            </FormGroup>

            <FormGroup>
              <FormLabel>{translate('Retention')}</FormLabel>

              <FormInputGroup
                type={inputTypes.NUMBER}
                name="retention"
                min={0}
                unit="days"
                helpText={translate('RetentionHelpText')}
                onChange={onInputChange}
                {...settings.retention}
              />
            </FormGroup>

            <FormGroup>
              <FormLabel>{translate('MaximumSize')}</FormLabel>

              <FormInputGroup
                type={inputTypes.NUMBER}
                name="maximumSize"
                min={0}
                unit="MB"
                helpText={translate('MaximumSizeHelpText')}
                onChange={onInputChange}
                {...settings.maximumSize}
              />
            </FormGroup>

            <FormGroup>
              <FormLabel>{translate('PreferIndexerFlags')}</FormLabel>

              <FormInputGroup
                type={inputTypes.CHECK}
                name="preferIndexerFlags"
                helpText={translate('PreferIndexerFlagsHelpText')}
                helpLink="https://wiki.servarr.com/Definitions#Indexer_Flags"
                onChange={onInputChange}
                {...settings.preferIndexerFlags}
              />
            </FormGroup>

            <FormGroup>
              <FormLabel>{translate('AvailabilityDelay')}</FormLabel>

              <FormInputGroup
                type={inputTypes.NUMBER}
                name="availabilityDelay"
                unit="Days"
                helpText={translate('AvailabilityDelayHelpText')}
                onChange={onInputChange}
                {...settings.availabilityDelay}
              />
            </FormGroup>

            <FormGroup
              advancedSettings={advancedSettings}
              isAdvanced={true}
            >
              <FormLabel>{translate('RSSSyncInterval')}</FormLabel>

              <FormInputGroup
                type={inputTypes.NUMBER}
                name="rssSyncInterval"
                min={0}
                max={120}
                unit="minutes"
                helpText={translate('HelpText')}
                helpTextWarning={translate('RSSSyncIntervalHelpTextWarning')}
                helpLink="https://wiki.servarr.com/radarr/faq#how-does-radarr-work"
                onChange={onInputChange}
                {...settings.rssSyncInterval}
              />
            </FormGroup>

            <FormGroup
              advancedSettings={advancedSettings}
              isAdvanced={true}
            >
              <FormLabel>{translate('WhitelistedSubtitleTags')}</FormLabel>

              <FormInputGroup
                type={inputTypes.TEXT_TAG}
                name="whitelistedHardcodedSubs"
                helpText={translate('WhitelistedHardcodedSubsHelpText')}
                onChange={onInputChange}
                {...settings.whitelistedHardcodedSubs}
              />
            </FormGroup>

            <FormGroup
              advancedSettings={advancedSettings}
              isAdvanced={true}
            >
              <FormLabel>{translate('AllowHardcodedSubs')}</FormLabel>

              <FormInputGroup
                type={inputTypes.CHECK}
                name="allowHardcodedSubs"
                helpText={translate('AllowHardcodedSubsHelpText')}
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
