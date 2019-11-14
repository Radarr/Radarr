import PropTypes from 'prop-types';
import React from 'react';
import { icons, kinds, inputTypes } from 'Helpers/Props';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import FieldSet from 'Components/FieldSet';
import Form from 'Components/Form/Form';
import FormGroup from 'Components/Form/FormGroup';
import FormLabel from 'Components/Form/FormLabel';
import FormInputGroup from 'Components/Form/FormInputGroup';
import Icon from 'Components/Icon';
import FormInputButton from 'Components/Form/FormInputButton';

var URLSearchParams = require('url-search-params');

var params = new URLSearchParams(window.location.search);
var oauth = params.get('access');
var refresh = params.get('refresh');
if (oauth && refresh) {
  history.pushState('object', 'title', (window.location.href).replace(window.location.search, '')); // jshint ignore:line
}

function resetTraktTokens(props) {
  if (window.confirm('Proceed to trakt.tv for authentication?\nYou will then be redirected back here.')) {
    window.location = 'http://radarr.aeonlucid.com/v1/trakt/redirect?target=' + window.location.href; // eslint-disable-line prefer-template
  }
}

function TraktAuthentication(props) {
  const {
    isFetching,
    error,
    settings,
    hasSettings,
    onInputChange
  } = props;

  if (oauth && refresh && hasSettings && !isFetching && !error) {
    settings.traktAuthToken.value = oauth;
    settings.traktRefreshToken.value = refresh;
    settings.traktTokenExpiry.value = Math.floor(Date.now() / 1000) + 4838400;
    oauth = null;
    refresh = null;
  }

  return (
    <FieldSet legend="Trakt Authentication">
      {
        isFetching &&
        <LoadingIndicator />
      }

      {
        !isFetching && error &&
        <div>Unable to load Trakt Authentication Settings</div>
      }

      {
        hasSettings && !isFetching && !error &&
        <Form>
          <FormGroup>
            <FormLabel>Auth Token</FormLabel>

            <FormInputGroup
              type={inputTypes.TEXT}
              // readOnly={true}
              name="traktAuthToken"
              onChange={onInputChange}
              {...settings.traktAuthToken}
            />
          </FormGroup>
          <FormGroup>
            <FormLabel>Refresh Token</FormLabel>

            <FormInputGroup
              type={inputTypes.TEXT}
              // readOnly={true}
              name="traktRefreshToken"
              onChange={onInputChange}
              {...settings.traktRefreshToken}
              buttons={[
                <FormInputButton
                  key="refreshTraktTokensButton"
                  kind={kinds.DANGER}
                  title="Reset Trakt Tokens"
                  onPress={resetTraktTokens}
                >
                  <Icon
                    name={icons.REFRESH}
                  />
                </FormInputButton>
              ]}
            />
          </FormGroup>

        </Form>
      }
    </FieldSet>
  );
}

TraktAuthentication.propTypes = {
  advancedSettings: PropTypes.bool.isRequired,
  isFetching: PropTypes.bool.isRequired,
  error: PropTypes.object,
  settings: PropTypes.object.isRequired,
  hasSettings: PropTypes.bool.isRequired,
  onInputChange: PropTypes.func.isRequired
};

export default TraktAuthentication;
