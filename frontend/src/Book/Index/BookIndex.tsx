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
import { fetchBooks } from 'Store/Actions/bookActions';
import translate from 'Utilities/String/translate';
import BookIndexRow from './BookIndexRow';

const columns = [
  {
    name: 'title',
    label: () => translate('Title'),
    isVisible: true,
  },
  {
    name: 'author',
    label: () => translate('Author'),
    isVisible: true,
  },
  {
    name: 'publisher',
    label: () => translate('Publisher'),
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

function BookIndex() {
  const dispatch = useDispatch();
  const { isFetching, isPopulated, error, items } = useSelector(
    (state: AppState) => state.books
  );

  useEffect(() => {
    dispatch(fetchBooks());
  }, [dispatch]);

  const onRefreshPress = useCallback(() => {
    dispatch(fetchBooks());
  }, [dispatch]);

  const hasNoBooks = isPopulated && !items.length;

  return (
    <PageContent title={translate('Books')}>
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
          <Alert kind={kinds.DANGER}>{translate('UnableToLoadBooks')}</Alert>
        ) : null}

        {isPopulated && !error && items.length > 0 ? (
          <Table columns={columns}>
            <TableBody>
              {items.map((book) => (
                <BookIndexRow key={book.id} {...book} />
              ))}
            </TableBody>
          </Table>
        ) : null}

        {hasNoBooks ? (
          <div style={{ padding: '20px', textAlign: 'center' }}>
            <p>{translate('NoBooks')}</p>
            <p>Add books to start tracking your library.</p>
          </div>
        ) : null}
      </PageContentBody>
    </PageContent>
  );
}

export default BookIndex;
