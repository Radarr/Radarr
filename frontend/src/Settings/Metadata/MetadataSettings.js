import React, { Component } from 'react';
import PageContent from 'Components/Page/PageContent';
import PageContentBodyConnector from 'Components/Page/PageContentBodyConnector';
import SettingsToolbarConnector from 'Settings/SettingsToolbarConnector';
import MetadatasConnector from './Metadata/MetadatasConnector';
import MetadataProviderConnector from './MetadataProvider/MetadataProviderConnector';

class MetadataSettings extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      hasPendingChanges: false
    };
  }

  //
  // Listeners

  setMetadataProviderRef = (ref) => {
    this._metadataProvider = ref;
  }

  onHasPendingChange = (hasPendingChanges) => {
    this.setState({
      hasPendingChanges
    });
  }

  onSavePress = () => {
    this._metadataProvider.getWrappedInstance().save();
  }

  //
  // Render
  render() {
    return (
      <PageContent title="Metadata Settings">
        <SettingsToolbarConnector
          hasPendingChanges={this.state.hasPendingChanges}
          onSavePress={this.onSavePress}
        />

        <PageContentBodyConnector>
          <MetadatasConnector />
          <MetadataProviderConnector
            ref={this.setMetadataProviderRef}
            onHasPendingChange={this.onHasPendingChange}
          />
        </PageContentBodyConnector>
      </PageContent>
    );
  }
}

export default MetadataSettings;
