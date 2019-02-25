import _ from 'lodash';
import PropTypes from 'prop-types';
import React from 'react';
import { inputTypes } from 'Helpers/Props';
import FormGroup from 'Components/Form/FormGroup';
import FormLabel from 'Components/Form/FormLabel';
import FormInputGroup from 'Components/Form/FormInputGroup';

function getType(type) {
  switch (type) {
    case 'captcha':
      return inputTypes.CAPTCHA;
    case 'checkbox':
      return inputTypes.CHECK;
    case 'device':
      return inputTypes.DEVICE;
    case 'password':
      return inputTypes.PASSWORD;
    case 'number':
      return inputTypes.NUMBER;
    case 'path':
      return inputTypes.PATH;
    case 'select':
      return inputTypes.SELECT;
    case 'tag':
      return inputTypes.TEXT_TAG;
    case 'textbox':
      return inputTypes.TEXT;
    case 'oauth':
      return inputTypes.OAUTH;
    default:
      return inputTypes.TEXT;
  }
}

function getSelectValues(selectOptions) {
  if (!selectOptions) {
    return;
  }

  return _.reduce(selectOptions, (result, option) => {
    result.push({
      key: option.value,
      value: option.name
    });

    return result;
  }, []);
}

function ProviderFieldFormGroup(props) {
  const {
    advancedSettings,
    name,
    label,
    helpText,
    helpLink,
    value,
    type,
    advanced,
    pending,
    errors,
    warnings,
    selectOptions,
    onChange,
    ...otherProps
  } = props;

  return (
    <FormGroup
      advancedSettings={advancedSettings}
      isAdvanced={advanced}
    >
      <FormLabel>{label}</FormLabel>

      <FormInputGroup
        type={getType(type)}
        name={name}
        label={label}
        helpText={helpText}
        helpLink={helpLink}
        value={value}
        values={getSelectValues(selectOptions)}
        errors={errors}
        warnings={warnings}
        pending={pending}
        hasFileBrowser={false}
        onChange={onChange}
        {...otherProps}
      />
    </FormGroup>
  );
}

const selectOptionsShape = {
  name: PropTypes.string.isRequired,
  value: PropTypes.number.isRequired
};

ProviderFieldFormGroup.propTypes = {
  advancedSettings: PropTypes.bool.isRequired,
  name: PropTypes.string.isRequired,
  label: PropTypes.string.isRequired,
  helpText: PropTypes.string,
  helpLink: PropTypes.string,
  value: PropTypes.any,
  type: PropTypes.string.isRequired,
  advanced: PropTypes.bool.isRequired,
  pending: PropTypes.bool.isRequired,
  errors: PropTypes.arrayOf(PropTypes.object).isRequired,
  warnings: PropTypes.arrayOf(PropTypes.object).isRequired,
  selectOptions: PropTypes.arrayOf(PropTypes.shape(selectOptionsShape)),
  onChange: PropTypes.func.isRequired
};

ProviderFieldFormGroup.defaultProps = {
  advancedSettings: false
};

export default ProviderFieldFormGroup;
