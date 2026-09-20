import React, { useCallback } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { AUTO_TAG_MOVIES } from 'Commands/commandNames';
import PageContent from 'Components/Page/PageContent';
import PageContentBody from 'Components/Page/PageContentBody';
import SettingsToolbar from 'Settings/SettingsToolbar';
import translate from 'Utilities/String/translate';
import AutoTaggings from './AutoTagging/AutoTaggings';
import Tags from './Tags';

function TagSettings() {
  const dispatch = useDispatch();
  const isAutoTagging = useSelector(createCommandExecutingSelector(AUTO_TAG_MOVIES));

  const onRunAutoTagPress = useCallback(() => {
    dispatch(executeCommand({ name: AUTO_TAG_MOVIES }));
  }, [dispatch]);

  return (
    <PageContent title={translate('Tags')}>
      <SettingsToolbar showSave={false} />

      <PageContentBody>
        <Tags />
        <AutoTaggings />
      </PageContentBody>
    </PageContent>
  );
}

export default TagSettings;
