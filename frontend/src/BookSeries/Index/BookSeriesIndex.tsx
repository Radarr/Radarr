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
import { fetchBookSeries } from 'Store/Actions/bookSeriesActions';
import translate from 'Utilities/String/translate';
import BookSeriesIndexRow from './BookSeriesIndexRow';

const columns = [
  {
    name: 'title',
    label: () => translate('Title'),
    isVisible: true,
  },
  {
    name: 'description',
    label: () => translate('Description'),
    isVisible: true,
  },
  {
    name: 'monitored',
    label: () => translate('Monitored'),
    isVisible: true,
  },
];

function BookSeriesIndex() {
  const dispatch = useDispatch();
  const { isFetching, isPopulated, error, items } = useSelector(
    (state: AppState) => state.bookSeries
  );

  useEffect(() => {
    dispatch(fetchBookSeries());
  }, [dispatch]);

  const onRefreshPress = useCallback(() => {
    dispatch(fetchBookSeries());
  }, [dispatch]);

  const hasNoBookSeries = isPopulated && !items.length;

  return (
    <PageContent title={translate('BookSeries')}>
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
            {translate('UnableToLoadBookSeries')}
          </Alert>
        ) : null}

        {isPopulated && !error && items.length > 0 ? (
          <Table columns={columns}>
            <TableBody>
              {items.map((bookSeriesItem) => (
                <BookSeriesIndexRow
                  key={bookSeriesItem.id}
                  {...bookSeriesItem}
                />
              ))}
            </TableBody>
          </Table>
        ) : null}

        {hasNoBookSeries ? (
          <div style={{ padding: '20px', textAlign: 'center' }}>
            <p>{translate('NoBookSeries')}</p>
            <p>Add book series to organize books into collections.</p>
          </div>
        ) : null}
      </PageContentBody>
    </PageContent>
  );
}

export default BookSeriesIndex;
