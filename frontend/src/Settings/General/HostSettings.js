import PropTypes from 'prop-types';
import React from 'react';
import FieldSet from 'Components/FieldSet';
import FormGroup from 'Components/Form/FormGroup';
import FormInputGroup from 'Components/Form/FormInputGroup';
import FormLabel from 'Components/Form/FormLabel';
import { inputTypes, sizes } from 'Helpers/Props';
import translate from 'Utilities/String/translate';

function HostSettings(props) {
  const {
    advancedSettings,
    settings,
    isWindows,
    mode,
    onInputChange
  } = props;

  const {
    bindAddress,
    port,
    urlBase,
    enableSsl,
    sslPort,
    sslCertPath,
    sslCertPassword,
    launchBrowser
  } = settings;

  return (
    <FieldSet legend={translate('Host')}>
      <FormGroup
        advancedSettings={advancedSettings}
        isAdvanced={true}
      >
        <FormLabel>{translate('BindAddress')}</FormLabel>

        <FormInputGroup
          type={inputTypes.TEXT}
          name="bindAddress"
          helpText={translate('BindAddressHelpText')}
          helpTextWarning={translate('RestartRequiredHelpTextWarning')}
          onChange={onInputChange}
          {...bindAddress}
        />
      </FormGroup>

      <FormGroup>
        <FormLabel>{translate('PortNumber')}</FormLabel>

        <FormInputGroup
          type={inputTypes.NUMBER}
          name="port"
          min={1}
          max={65535}
          helpTextWarning={translate('RestartRequiredHelpTextWarning')}
          onChange={onInputChange}
          {...port}
        />
      </FormGroup>

      <FormGroup>
        <FormLabel>{translate('URLBase')}</FormLabel>

        <FormInputGroup
          type={inputTypes.TEXT}
          name="urlBase"
          helpText={translate('UrlBaseHelpText')}
          helpTextWarning={translate('RestartRequiredHelpTextWarning')}
          onChange={onInputChange}
          {...urlBase}
        />
      </FormGroup>

      <FormGroup
        advancedSettings={advancedSettings}
        isAdvanced={true}
        size={sizes.MEDIUM}
      >
        <FormLabel>{translate('EnableSSL')}</FormLabel>

        <FormInputGroup
          type={inputTypes.CHECK}
          name="enableSsl"
          helpText={translate('EnableSslHelpText')}
          onChange={onInputChange}
          {...enableSsl}
        />
      </FormGroup>

      {
        enableSsl.value ?
          <FormGroup
            advancedSettings={advancedSettings}
            isAdvanced={true}
          >
            <FormLabel>{translate('SSLPort')}</FormLabel>

            <FormInputGroup
              type={inputTypes.NUMBER}
              name="sslPort"
              min={1}
              max={65535}
              helpTextWarning={translate('RestartRequiredHelpTextWarning')}
              onChange={onInputChange}
              {...sslPort}
            />
          </FormGroup> :
          null
      }

      {
        enableSsl.value ?
          <FormGroup
            advancedSettings={advancedSettings}
            isAdvanced={true}
          >
            <FormLabel>{translate('SSLCertPath')}</FormLabel>

            <FormInputGroup
              type={inputTypes.TEXT}
              name="sslCertPath"
              helpText={translate('SSLCertPathHelpText')}
              helpTextWarning={translate('RestartRequiredHelpTextWarning')}
              onChange={onInputChange}
              {...sslCertPath}
            />
          </FormGroup> :
          null
      }

      {
        enableSsl.value ?
          <FormGroup
            advancedSettings={advancedSettings}
            isAdvanced={true}
          >
            <FormLabel>{translate('SSLCertPassword')}</FormLabel>

            <FormInputGroup
              type={inputTypes.PASSWORD}
              name="sslCertPassword"
              helpText={translate('SSLCertPasswordHelpText')}
              helpTextWarning={translate('RestartRequiredHelpTextWarning')}
              onChange={onInputChange}
              {...sslCertPassword}
            />
          </FormGroup> :
          null
      }

      {
        isWindows && mode !== 'service' &&
          <FormGroup size={sizes.MEDIUM}>
            <FormLabel>{translate('OpenBrowserOnStart')}</FormLabel>

            <FormInputGroup
              type={inputTypes.CHECK}
              name="launchBrowser"
              helpText={translate('LaunchBrowserHelpText')}
              onChange={onInputChange}
              {...launchBrowser}
            />
          </FormGroup>
      }

    </FieldSet>
  );
}

HostSettings.propTypes = {
  advancedSettings: PropTypes.bool.isRequired,
  settings: PropTypes.object.isRequired,
  isWindows: PropTypes.bool.isRequired,
  mode: PropTypes.string.isRequired,
  onInputChange: PropTypes.func.isRequired
};

export default HostSettings;
