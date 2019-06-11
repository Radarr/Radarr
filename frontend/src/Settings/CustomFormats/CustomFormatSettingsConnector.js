import React, { Component } from 'react';
import { DragDropContext } from 'react-dnd';
import HTML5Backend from 'react-dnd-html5-backend';
import PageContent from 'Components/Page/PageContent';
import PageContentBodyConnector from 'Components/Page/PageContentBodyConnector';
import CustomFormatsConnector from './CustomFormats/CustomFormatsConnector';
import SettingsToolbarConnector from 'Settings/SettingsToolbarConnector';

class CustomFormatSettingsConnector extends Component {

  //
  // Render

  render() {
    return (
      <PageContent title="Custom Formats Settings">
        <SettingsToolbarConnector
          showSave={false}
        />

        <PageContentBodyConnector>
          <CustomFormatsConnector />
        </PageContentBodyConnector>
      </PageContent>
    );
  }
}

export default DragDropContext(HTML5Backend)(CustomFormatSettingsConnector);

