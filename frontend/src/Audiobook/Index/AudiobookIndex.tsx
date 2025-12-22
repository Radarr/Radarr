import React, { useCallback, useEffect } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import AppState from 'App/State/AppState';
import Alert from 'Components/Alert';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import PageContent from 'Components/Page/PageContent';
import PageContentBody from 'Components/Page/PageContentBody';
import PageToolbar from 'Components/Page/Toolbar/PageToolbar';
import PageToolbarButton from 'Components/Page/Toolbar/PageToolbarButton';
import PageToolbarSection from 'Components/Page/Toolbar/PageToolbarSection';
import Table from 'Components/Table/Table';
import TableBody from 'Components/Table/TableBody';
import { icons, kinds } from 'Helpers/Props';
import { fetchAudiobooks } from 'Store/Actions/audiobookActions';
import translate from 'Utilities/String/translate';
import AudiobookIndexRow from './AudiobookIndexRow';

const columns = [
  {
    name: 'title',
    label: () => translate('Title'),
    isVisible: true,
  },
  {
    name: 'narrator',
    label: () => translate('Narrator'),
    isVisible: true,
  },
  {
    name: 'duration',
    label: () => translate('Duration'),
    isVisible: true,
  },
  {
    name: 'releaseDate',
    label: () => translate('ReleaseDate'),
    isVisible: true,
  },
  {
    name: 'monitored',
    label: () => translate('Monitored'),
    isVisible: true,
  },
];

function AudiobookIndex() {
  const dispatch = useDispatch();
  const { isFetching, isPopulated, error, items } = useSelector(
    (state: AppState) => state.audiobooks
  );

  useEffect(() => {
    dispatch(fetchAudiobooks());
  }, [dispatch]);

  const onRefreshPress = useCallback(() => {
    dispatch(fetchAudiobooks());
  }, [dispatch]);

  const hasNoAudiobooks = isPopulated && !items.length;

  return (
    <PageContent title={translate('Audiobooks')}>
      <PageToolbar>
        <PageToolbarSection>
          <PageToolbarButton
            label={translate('RefreshAll')}
            iconName={icons.REFRESH}
            isSpinning={isFetching}
            onPress={onRefreshPress}
          />
        </PageToolbarSection>
      </PageToolbar>

      <PageContentBody>
        {isFetching && !isPopulated ? <LoadingIndicator /> : null}

        {!isFetching && !!error ? (
          <Alert kind={kinds.DANGER}>
            {translate('UnableToLoadAudiobooks')}
          </Alert>
        ) : null}

        {isPopulated && !error && items.length > 0 ? (
          <Table columns={columns}>
            <TableBody>
              {items.map((audiobook) => (
                <AudiobookIndexRow key={audiobook.id} {...audiobook} />
              ))}
            </TableBody>
          </Table>
        ) : null}

        {hasNoAudiobooks ? (
          <div style={{ padding: '20px', textAlign: 'center' }}>
            <p>{translate('NoAudiobooks')}</p>
            <p>Add audiobooks to start tracking your library.</p>
          </div>
        ) : null}
      </PageContentBody>
    </PageContent>
  );
}

export default AudiobookIndex;
