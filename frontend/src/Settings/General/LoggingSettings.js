import PropTypes from 'prop-types';
import React from 'react';
import FieldSet from 'Components/FieldSet';
import FormGroup from 'Components/Form/FormGroup';
import FormInputGroup from 'Components/Form/FormInputGroup';
import FormLabel from 'Components/Form/FormLabel';
import { inputTypes } from 'Helpers/Props';
import translate from 'Utilities/String/translate';

const logLevelOptions = [
  { key: 'fatal', value: translate('Fatal') },
  { key: 'error', value: translate('Error') },
  { key: 'warn', value: translate('Warn') },
  { key: 'info', value: translate('Info') },
  { key: 'debug', value: translate('Debug') },
  { key: 'trace', value: translate('Trace') }
];

function LoggingSettings(props) {
  const {
    settings,
    onInputChange
  } = props;

  const {
    logLevel
  } = settings;

  return (
    <FieldSet legend={translate('Logging')}>
      <FormGroup>
        <FormLabel>{translate('LogLevel')}</FormLabel>

        <FormInputGroup
          type={inputTypes.SELECT}
          name="logLevel"
          values={logLevelOptions}
          helpTextWarning={logLevel.value === 'trace' ? translate('LogLevelTraceHelpTextWarning') : undefined}
          onChange={onInputChange}
          {...logLevel}
        />
      </FormGroup>
    </FieldSet>
  );
}

LoggingSettings.propTypes = {
  settings: PropTypes.object.isRequired,
  onInputChange: PropTypes.func.isRequired
};

export default LoggingSettings;
