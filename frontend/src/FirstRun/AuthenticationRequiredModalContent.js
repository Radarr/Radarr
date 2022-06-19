import PropTypes from 'prop-types';
import React, { useEffect, useRef } from 'react';
import Alert from 'Components/Alert';
import FormGroup from 'Components/Form/FormGroup';
import FormInputButton from 'Components/Form/FormInputButton';
import FormInputGroup from 'Components/Form/FormInputGroup';
import FormLabel from 'Components/Form/FormLabel';
import OAuthInputConnector from 'Components/Form/OAuthInputConnector';
import Icon from 'Components/Icon';
import SpinnerButton from 'Components/Link/SpinnerButton';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import { icons, inputTypes, kinds } from 'Helpers/Props';
import { authenticationMethodOptions, authenticationRequiredOptions, authenticationRequiredWarning } from 'Settings/General/SecuritySettings';
import styles from './AuthenticationRequiredModalContent.css';

const oauthData = {
  implementation: { value: 'PlexImport' },
  configContract: { value: 'PlexListSettings' },
  fields: [
    {
      type: 'textbox',
      name: 'accessToken'
    },
    {
      type: 'oAuth',
      name: 'signIn',
      value: 'startAuth'
    }
  ]
};

function onModalClose() {
  // No-op
}

function AuthenticationRequiredModalContent(props) {
  const {
    isPopulated,
    plexServersPopulated,
    error,
    isSaving,
    settings,
    onInputChange,
    onSavePress,
    dispatchFetchStatus
  } = props;

  const {
    authenticationMethod,
    authenticationRequired,
    username,
    password,
    plexAuthServer,
    plexRequireOwner,
    oidcClientId,
    oidcClientSecret,
    oidcAuthority
  } = settings;

  const authenticationEnabled = authenticationMethod && authenticationMethod.value !== 'none';
  const showUserPass = authenticationMethod && ['basic', 'forms'].includes(authenticationMethod.value);
  const plexEnabled = authenticationMethod && authenticationMethod.value === 'plex';
  const oidcEnabled = authenticationMethod && authenticationMethod.value === 'oidc';

  const didMount = useRef(false);

  useEffect(() => {
    if (!isSaving && didMount.current) {
      dispatchFetchStatus();
    }

    didMount.current = true;
  }, [isSaving, dispatchFetchStatus]);

  return (
    <ModalContent
      showCloseButton={false}
      onModalClose={onModalClose}
    >
      <ModalHeader>
        Authentication Required
      </ModalHeader>

      <ModalBody>
        <Alert
          className={styles.authRequiredAlert}
          kind={kinds.WARNING}
        >
          {authenticationRequiredWarning}
        </Alert>

        {
          isPopulated && !error ?
            <div>
              <FormGroup>
                <FormLabel>Authentication</FormLabel>

                <FormInputGroup
                  type={inputTypes.SELECT}
                  name="authenticationMethod"
                  values={authenticationMethodOptions}
                  helpText="Require login to access Sonarr"
                  onChange={onInputChange}
                  {...authenticationMethod}
                />
              </FormGroup>

              {
                authenticationEnabled ?
                  <FormGroup>
                    <FormLabel>Authentication Required</FormLabel>

                    <FormInputGroup
                      type={inputTypes.SELECT}
                      name="authenticationRequired"
                      values={authenticationRequiredOptions}
                      helpText="Change which requests authentication is required for. Do not change unless you understand the risks."
                      onChange={onInputChange}
                      {...authenticationRequired}
                    />
                  </FormGroup> :
                  null
              }

              {
                showUserPass &&
                  <>
                    <FormGroup>
                      <FormLabel>Username</FormLabel>

                      <FormInputGroup
                        type={inputTypes.TEXT}
                        name="username"
                        onChange={onInputChange}
                        {...username}
                      />
                    </FormGroup>

                    <FormGroup>
                      <FormLabel>Password</FormLabel>

                      <FormInputGroup
                        type={inputTypes.PASSWORD}
                        name="password"
                        onChange={onInputChange}
                        {...password}
                      />
                    </FormGroup>
                  </>
              }

              {
                plexEnabled &&
                  <>
                    <FormGroup>
                      <FormLabel>Plex Server</FormLabel>

                      <FormInputGroup
                        type={inputTypes.PLEX_MACHINE_SELECT}
                        name="plexAuthServer"
                        buttons={[
                          <FormInputButton
                            key="auth"
                            ButtonComponent={OAuthInputConnector}
                            label={plexServersPopulated ? <Icon name={icons.REFRESH} /> : 'Fetch'}
                            name="plexAuth"
                            provider="importList"
                            providerData={oauthData}
                            section="settings.importLists"
                            onChange={onInputChange}
                          />
                        ]}
                        onChange={onInputChange}
                        {...plexAuthServer}
                      />
                    </FormGroup>

                    <FormGroup>
                      <FormLabel>Restrict Access to Server Owner</FormLabel>

                      <FormInputGroup
                        type={inputTypes.CHECK}
                        name="plexRequireOwner"
                        onChange={onInputChange}
                        {...plexRequireOwner}
                      />
                    </FormGroup>
                  </>
              }

              {
                oidcEnabled &&
                  <>
                    <FormGroup>
                      <FormLabel>Authority</FormLabel>

                      <FormInputGroup
                        type={inputTypes.TEXT}
                        name="oidcAuthority"
                        onChange={onInputChange}
                        {...oidcAuthority}
                      />
                    </FormGroup>

                    <FormGroup>
                      <FormLabel>ClientId</FormLabel>

                      <FormInputGroup
                        type={inputTypes.TEXT}
                        name="oidcClientId"
                        onChange={onInputChange}
                        {...oidcClientId}
                      />
                    </FormGroup>

                    <FormGroup>
                      <FormLabel>ClientSecret</FormLabel>

                      <FormInputGroup
                        type={inputTypes.PASSWORD}
                        name="oidcClientSecret"
                        onChange={onInputChange}
                        {...oidcClientSecret}
                      />
                    </FormGroup>
                  </>
              }
            </div> :
            null
        }

        {
          !isPopulated && !error ? <LoadingIndicator /> : null
        }
      </ModalBody>

      <ModalFooter>
        <SpinnerButton
          kind={kinds.PRIMARY}
          isSpinning={isSaving}
          isDisabled={!authenticationEnabled}
          onPress={onSavePress}
        >
          Save
        </SpinnerButton>
      </ModalFooter>
    </ModalContent>
  );
}

AuthenticationRequiredModalContent.propTypes = {
  isPopulated: PropTypes.bool.isRequired,
  plexServersPopulated: PropTypes.bool.isRequired,
  error: PropTypes.object,
  isSaving: PropTypes.bool.isRequired,
  saveError: PropTypes.object,
  settings: PropTypes.object.isRequired,
  onInputChange: PropTypes.func.isRequired,
  onSavePress: PropTypes.func.isRequired,
  dispatchFetchStatus: PropTypes.func.isRequired
};

export default AuthenticationRequiredModalContent;
