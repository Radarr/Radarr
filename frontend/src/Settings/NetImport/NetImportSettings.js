import PropTypes from 'prop-types';
import React, { Component, Fragment } from 'react';
import PageContent from 'Components/Page/PageContent';
import PageContentBody from 'Components/Page/PageContentBody';
import PageToolbarButton from 'Components/Page/Toolbar/PageToolbarButton';
import PageToolbarSeparator from 'Components/Page/Toolbar/PageToolbarSeparator';
import { icons } from 'Helpers/Props';
import SettingsToolbarConnector from 'Settings/SettingsToolbarConnector';
import NetImportsConnector from './NetImport/NetImportsConnector';
import NetImportExclusionsConnector from './NetImportExclusions/NetImportExclusionsConnector';
import NetImportOptionsConnector from './Options/NetImportOptionsConnector';

class NetImportSettings extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this._saveCallback = null;

    this.state = {
      isSaving: false,
      hasPendingChanges: false
    };
  }

  //
  // Listeners

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

        <PageContentBody>
          <NetImportsConnector />

          <NetImportOptionsConnector
            onChildMounted={this.onChildMounted}
            onChildStateChange={this.onChildStateChange}
          />

          <NetImportExclusionsConnector />

        </PageContentBody>
      </PageContent>
    );
  }
}

NetImportSettings.propTypes = {
  isTestingAll: PropTypes.bool.isRequired,
  dispatchTestAllNetImport: PropTypes.func.isRequired
};

export default NetImportSettings;
