import React, { Component } from 'react';
import { DndProvider } from 'react-dnd';
import { HTML5Backend } from 'react-dnd-html5-backend';
import Link from 'Components/Link/Link';
import PageContent from 'Components/Page/PageContent';
import PageContentBody from 'Components/Page/PageContentBody';
import SettingsToolbarConnector from 'Settings/SettingsToolbarConnector';
import translate from 'Utilities/String/translate';
import DelayProfilesConnector from './Delay/DelayProfilesConnector';
import QualityProfilesConnector from './Quality/QualityProfilesConnector';
import styles from './Profiles.css';
// Only a single DragDrop Context can exist so it's done here to allow editing
// quality profiles and reordering delay profiles to work.

class Profiles extends Component {

  //
  // Render

  render() {
    return (
      <PageContent title={translate('Profiles')}>
        <SettingsToolbarConnector
          showSave={false}
        />

        <PageContentBody>
          <DndProvider backend={HTML5Backend}>
            <QualityProfilesConnector />
            <DelayProfilesConnector />
            <div className={styles.addCustomFormatMessage}>
              {translate('LookingForReleaseProfiles1')}
              <Link to='/settings/customformats'> {translate('CustomFormats')} </Link>
              {translate('LookingForReleaseProfiles2')}
            </div>
          </DndProvider>
        </PageContentBody>
      </PageContent>
    );
  }
}

export default Profiles;
