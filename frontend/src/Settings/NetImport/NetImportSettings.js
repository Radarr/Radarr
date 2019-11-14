import PropTypes from 'prop-types';
import React, { Component, Fragment } from 'react';
import { icons } from 'Helpers/Props';
import PageContent from 'Components/Page/PageContent';
import PageContentBodyConnector from 'Components/Page/PageContentBodyConnector';
import PageToolbarButton from 'Components/Page/Toolbar/PageToolbarButton';
import PageToolbarSeparator from 'Components/Page/Toolbar/PageToolbarSeparator';
import SettingsToolbarConnector from 'Settings/SettingsToolbarConnector';
import NetImportsConnector from './NetImport/NetImportsConnector';
import NetImportOptionsConnector from './Options/NetImportOptionsConnector';
import TraktAuthenticationConnector from './TraktAuthentication/TraktAuthenticationConnector';
import NetImportExclusionsConnector from './NetImportExclusions/NetImportExclusionsConnector';

class NetImportSettings extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this._saveCallback = null;
    this._saveCallback2 = null;

    this.state2 = {
      isSaving: false,
      hasPendingChanges: false
    };

    this.state = {
      isSaving: false,
      hasPendingChanges: false
    };
  }

  //
  // Listeners

  onChildMounted2 = (saveCallback) => {
    this._saveCallback2 = saveCallback;
  }

  onChildMounted = (saveCallback) => {
    this._saveCallback = saveCallback;
  }

  onChildStateChange = (payload) => {
    this.setState(payload);
  }

  onSavePress = () => {
    if (this._saveCallback) {
      this._saveCallback();
    }
    if (this._saveCallback2) {
      this._saveCallback2();
    }
  }

  // Render
  //

  render() {
    const {
      isTestingAll,
      dispatchTestAllNetImport
    } = this.props;

    const {
      isSaving,
      hasPendingChanges
    } = this.state;

    return (
      <PageContent title="List Settings">
        <SettingsToolbarConnector
          isSaving={isSaving}
          hasPendingChanges={hasPendingChanges}
          additionalButtons={
            <Fragment>
              <PageToolbarSeparator />

              <PageToolbarButton
                label="Test All Lists"
                iconName={icons.TEST}
                isSpinning={isTestingAll}
                onPress={dispatchTestAllNetImport}
              />
            </Fragment>
          }
          onSavePress={this.onSavePress}
        />

        <PageContentBodyConnector>
          <NetImportsConnector />

          <NetImportOptionsConnector
            onChildMounted={this.onChildMounted}
            onChildStateChange={this.onChildStateChange}
          />

          <TraktAuthenticationConnector
            onChildMounted={this.onChildMounted2}
            onChildStateChange={this.onChildStateChange}
          />

          <NetImportExclusionsConnector />

        </PageContentBodyConnector>
      </PageContent>
    );
  }
}

NetImportSettings.propTypes = {
  isTestingAll: PropTypes.bool.isRequired,
  dispatchTestAllNetImport: PropTypes.func.isRequired
};

export default NetImportSettings;
