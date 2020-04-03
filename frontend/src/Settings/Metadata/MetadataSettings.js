import React, { Component } from 'react';
import PageContent from 'Components/Page/PageContent';
import PageContentBodyConnector from 'Components/Page/PageContentBodyConnector';
import SettingsToolbarConnector from 'Settings/SettingsToolbarConnector';
import MetadatasConnector from './Metadata/MetadatasConnector';
import MetadataOptionsConnector from './Options/MetadataOptionsConnector';

class MetadataSettings extends Component {

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

  render() {
    const {
      isSaving,
      hasPendingChanges
    } = this.state;

    return (
      <PageContent title="Metadata Settings">
        <SettingsToolbarConnector
          isSaving={isSaving}
          hasPendingChanges={hasPendingChanges}
          onSavePress={this.onSavePress}
        />

        <PageContentBodyConnector>
          <MetadataOptionsConnector
            onChildMounted={this.onChildMounted}
            onChildStateChange={this.onChildStateChange}
          />

          <MetadatasConnector />
        </PageContentBodyConnector>
      </PageContent>
    );
  }
}

export default MetadataSettings;
