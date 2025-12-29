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
import { fetchTVShows } from 'Store/Actions/tvShowActions';
import translate from 'Utilities/String/translate';
import TVShowIndexRow from './TVShowIndexRow';

const columns = [
  {
    name: 'title',
    label: () => translate('Title'),
    isVisible: true,
  },
  {
    name: 'network',
    label: () => translate('Network'),
    isVisible: true,
  },
  {
    name: 'status',
    label: () => translate('Status'),
    isVisible: true,
  },
  {
    name: 'year',
    label: () => translate('Year'),
    isVisible: true,
  },
  {
    name: 'monitored',
    label: () => translate('Monitored'),
    isVisible: true,
  },
];

function TVShowIndex() {
  const dispatch = useDispatch();
  const { isFetching, isPopulated, error, items } = useSelector(
    (state: AppState) => state.tvShows
  );

  useEffect(() => {
    dispatch(fetchTVShows());
  }, [dispatch]);

  const onRefreshPress = useCallback(() => {
    dispatch(fetchTVShows());
  }, [dispatch]);

  const hasNoTVShows = isPopulated && !items.length;

  return (
    <PageContent title={translate('TVShows')}>
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
          <Alert kind={kinds.DANGER}>{translate('UnableToLoadTVShows')}</Alert>
        ) : null}

        {isPopulated && !error && items.length > 0 ? (
          <Table columns={columns}>
            <TableBody>
              {items.map((tvShow) => (
                <TVShowIndexRow key={tvShow.id} {...tvShow} />
              ))}
            </TableBody>
          </Table>
        ) : null}

        {hasNoTVShows ? (
          <div style={{ padding: '20px', textAlign: 'center' }}>
            <p>{translate('NoTVShows')}</p>
            <p>Add TV shows to track your series.</p>
          </div>
        ) : null}
      </PageContentBody>
    </PageContent>
  );
}

export default TVShowIndex;
