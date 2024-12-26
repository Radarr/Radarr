import React, { Component } from 'react';
import PageContent from 'Components/Page/PageContent';
import PageContentBody from 'Components/Page/PageContentBody';
import SettingsToolbar from 'Settings/SettingsToolbar';
import translate from 'Utilities/String/translate';
import Metadatas from './Metadata/Metadatas';
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
  };

  onChildStateChange = (payload) => {
    this.setState(payload);
  };

  onSavePress = () => {
    if (this._saveCallback) {
      this._saveCallback();
    }
  };

  //
  // Render

  render() {
    const {
      isSaving,
      hasPendingChanges
    } = this.state;

    return (
      <PageContent title={translate('MetadataSettings')}>
        <SettingsToolbar
          isSaving={isSaving}
          hasPendingChanges={hasPendingChanges}
          onSavePress={this.onSavePress}
        />

        <PageContentBody>
          <MetadataOptionsConnector
            onChildMounted={this.onChildMounted}
            onChildStateChange={this.onChildStateChange}
          />

          <Metadatas />
        </PageContentBody>
      </PageContent>
    );
  }
}

export default MetadataSettings;
