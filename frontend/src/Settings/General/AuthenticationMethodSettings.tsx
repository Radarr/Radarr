import React from 'react';
import { InputChanged } from 'typings/inputs';
import { PendingSection } from 'typings/pending';
import General from 'typings/Settings/General';
import FormsAuthenticationSettings from './FormsAuthenticationSettings';
import OidcAuthenticationSettings from './OidcAuthenticationSettings';

interface AuthenticationMethodSettingsProps {
  authenticationMethod: PendingSection<General>['authenticationMethod'];
  username: PendingSection<General>['username'];
  password: PendingSection<General>['password'];
  passwordConfirmation: PendingSection<General>['passwordConfirmation'];
  oidcAuthority: PendingSection<General>['oidcAuthority'];
  oidcClientId: PendingSection<General>['oidcClientId'];
  oidcClientSecret: PendingSection<General>['oidcClientSecret'];
  oidcUserIdentifier: PendingSection<General>['oidcUserIdentifier'];
  oidcScopes: PendingSection<General>['oidcScopes'];
  showValidationWarnings?: boolean;
  onInputChange: (change: InputChanged) => void;
}

function AuthenticationMethodSettings({
  authenticationMethod,
  username,
  password,
  passwordConfirmation,
  oidcAuthority,
  oidcClientId,
  oidcClientSecret,
  oidcUserIdentifier,
  oidcScopes,
  showValidationWarnings,
  onInputChange,
}: AuthenticationMethodSettingsProps) {
  switch (authenticationMethod?.value) {
    case 'forms':
      return (
        <FormsAuthenticationSettings
          username={username}
          password={password}
          passwordConfirmation={passwordConfirmation}
          showValidationWarnings={showValidationWarnings}
          onInputChange={onInputChange}
        />
      );

    case 'oidc':
      return (
        <OidcAuthenticationSettings
          oidcAuthority={oidcAuthority}
          oidcClientId={oidcClientId}
          oidcClientSecret={oidcClientSecret}
          oidcUserIdentifier={oidcUserIdentifier}
          oidcScopes={oidcScopes}
          showValidationWarnings={showValidationWarnings}
          onInputChange={onInputChange}
        />
      );

    default:
      return null;
  }
}

export default AuthenticationMethodSettings;
