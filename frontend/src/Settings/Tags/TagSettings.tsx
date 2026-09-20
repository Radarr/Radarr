import React, { useCallback } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { AUTO_TAG_MOVIES } from 'Commands/commandNames';
import PageContent from 'Components/Page/PageContent';
import PageContentBody from 'Components/Page/PageContentBody';
import PageToolbarButton from 'Components/Page/Toolbar/PageToolbarButton';
import PageToolbarSeparator from 'Components/Page/Toolbar/PageToolbarSeparator';
import { icons } from 'Helpers/Props';
import SettingsToolbar from 'Settings/SettingsToolbar';
import { executeCommand } from 'Store/Actions/commandActions';
import createCommandExecutingSelector from 'Store/Selectors/createCommandExecutingSelector';
import translate from 'Utilities/String/translate';
import AutoTaggings from './AutoTagging/AutoTaggings';
import Tags from './Tags';

function TagSettings() {
  const dispatch = useDispatch();
  const isAutoTagging = useSelector(
    createCommandExecutingSelector(AUTO_TAG_MOVIES)
  );

  const onRunAutoTagPress = useCallback(() => {
    dispatch(executeCommand({ name: AUTO_TAG_MOVIES }));
  }, [dispatch]);

  return (
    <PageContent title={translate('Tags')}>
      <SettingsToolbar
        showSave={false}
        additionalButtons={
          <>
            <PageToolbarSeparator />

            <PageToolbarButton
              label={translate('RunAutoTag')}
              iconName={icons.TAGS}
              isSpinning={isAutoTagging}
              onPress={onRunAutoTagPress}
            />
          </>
        }
      />

      <PageContentBody>
        <Tags />
        <AutoTaggings />
      </PageContentBody>
    </PageContent>
  );
}

export default TagSettings;
