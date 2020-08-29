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

function NetImportOptions(props) {
  const {
    isFetching,
    error,
    settings,
    hasSettings,
    onInputChange
  } = props;

  const cleanLibraryLevelOptions = [
    { key: 'disabled', value: 'Disabled' },
    { key: 'logOnly', value: 'Log Only' },
    { key: 'keepAndUnmonitor', value: 'Keep and Unmonitor' },
    { key: 'removeAndKeep', value: 'Remove and Keep' },
    { key: 'removeAndDelete', value: 'Remove and Delete' }
  ];

  return (
    <FieldSet legend={translate('Options')}>
      {
        isFetching &&
          <LoadingIndicator />
      }

      {
        !isFetching && error &&
          <div>Unable to load list options</div>
      }

      {
        hasSettings && !isFetching && !error &&
          <Form>
            <FormGroup>
              <FormLabel>{translate('ListUpdateInterval')}</FormLabel>

              <FormInputGroup
                type={inputTypes.NUMBER}
                name="netImportSyncInterval"
                min={0}
                unit="minutes"
                helpText={translate('NetImportSyncIntervalHelpText')}
                onChange={onInputChange}
                {...settings.netImportSyncInterval}
              />
            </FormGroup>

            <FormGroup>
              <FormLabel>{translate('CleanLibraryLevel')}</FormLabel>

              <FormInputGroup
                type={inputTypes.SELECT}
                name="listSyncLevel"
                values={cleanLibraryLevelOptions}
                helpText={translate('ListSyncLevelHelpText')}
                onChange={onInputChange}
                {...settings.listSyncLevel}
              />
            </FormGroup>
          </Form>
      }
    </FieldSet>
  );
}

NetImportOptions.propTypes = {
  advancedSettings: PropTypes.bool.isRequired,
  isFetching: PropTypes.bool.isRequired,
  error: PropTypes.object,
  settings: PropTypes.object.isRequired,
  hasSettings: PropTypes.bool.isRequired,
  onInputChange: PropTypes.func.isRequired
};

export default NetImportOptions;
