import React, { Component } from 'react';
import { DndProvider } from 'react-dnd';
import HTML5Backend from 'react-dnd-html5-backend';
import PageContent from 'Components/Page/PageContent';
import PageContentBodyConnector from 'Components/Page/PageContentBodyConnector';
import Link from 'Components/Link/Link';
import SettingsToolbarConnector from 'Settings/SettingsToolbarConnector';
import QualityProfilesConnector from './Quality/QualityProfilesConnector';
import DelayProfilesConnector from './Delay/DelayProfilesConnector';
import styles from './Profiles.css';
// Only a single DragDrop Context can exist so it's done here to allow editing
// quality profiles and reordering delay profiles to work.

class Profiles extends Component {

  //
  // Render

  render() {
    return (
      <PageContent title="Profiles">
        <SettingsToolbarConnector
          showSave={false}
        />

        <PageContentBodyConnector>
          <DndProvider backend={HTML5Backend}>
            <QualityProfilesConnector />
            <DelayProfilesConnector />
            <div className={styles.addCustomFormatMessage}>
              Looking for Release Profiles? Try
              <Link to='/settings/customformats'> Custom Formats </Link>
              instead.
            </div>
          </DndProvider>
        </PageContentBodyConnector>
      </PageContent>
    );
  }
}

export default Profiles;
