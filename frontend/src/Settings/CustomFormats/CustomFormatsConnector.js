import React from 'react';
import PageContent from 'Components/Page/PageContent';
import SettingsToolbarConnector from 'Settings/SettingsToolbarConnector';

function CustomFormatsConnector() {
  return (
    <PageContent title="Custom Formats Settings">
      <SettingsToolbarConnector
        showSave={false}
      />

    </PageContent>
  );
}

export default CustomFormatsConnector;

