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

function ImportListOptions(props) {
  const {
    advancedSettings,
    isFetching,
    error,
    settings,
    hasSettings,
    onInputChange
  } = props;

  const cleanLibraryLevelOptions = [
    { key: 'disabled', value: translate('Disabled') },
    { key: 'logOnly', value: translate('LogOnly') },
    { key: 'keepAndUnmonitor', value: translate('KeepAndUnmonitorMovie') },
    { key: 'removeAndKeep', value: translate('RemoveMovieAndKeepFiles') },
    { key: 'removeAndDelete', value: translate('RemoveMovieAndDeleteFiles') }
  ];

  return (
    advancedSettings &&
      <FieldSet legend={translate('Options')}>
        {
          isFetching &&
            <LoadingIndicator />
        }

        {
          !isFetching && error &&
            <div>
              {translate('UnableToLoadListOptions')}
            </div>
        }

        {
          hasSettings && !isFetching && !error &&
            <Form>
              <FormGroup
                advancedSettings={advancedSettings}
                isAdvanced={true}
              >
                <FormLabel>{translate('ListUpdateInterval')}</FormLabel>

                <FormInputGroup
                  type={inputTypes.NUMBER}
                  name="importListSyncInterval"
                  min={6}
                  unit="hours"
                  helpText={translate('ImportListSyncIntervalHelpText')}
                  onChange={onInputChange}
                  {...settings.importListSyncInterval}
                />
              </FormGroup>

              <FormGroup
                advancedSettings={advancedSettings}
                isAdvanced={true}
              >
                <FormLabel>{translate('CleanLibraryLevel')}</FormLabel>

                <FormInputGroup
                  type={inputTypes.SELECT}
                  name="listSyncLevel"
                  values={cleanLibraryLevelOptions}
                  helpText={translate('ListSyncLevelHelpText')}
                  helpTextWarning={settings.listSyncLevel.value === 'removeAndDelete' ? translate('ListSyncLevelHelpTextWarning') : undefined}
                  onChange={onInputChange}
                  {...settings.listSyncLevel}
                />
              </FormGroup>
            </Form>
        }
      </FieldSet>
  );
}

ImportListOptions.propTypes = {
  advancedSettings: PropTypes.bool.isRequired,
  isFetching: PropTypes.bool.isRequired,
  error: PropTypes.object,
  settings: PropTypes.object.isRequired,
  hasSettings: PropTypes.bool.isRequired,
  onInputChange: PropTypes.func.isRequired
};

export default ImportListOptions;
