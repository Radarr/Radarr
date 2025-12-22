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
import { fetchAuthors } from 'Store/Actions/authorActions';
import translate from 'Utilities/String/translate';
import AuthorIndexRow from './AuthorIndexRow';

const columns = [
  {
    name: 'name',
    label: () => translate('Name'),
    isVisible: true,
  },
  {
    name: 'path',
    label: () => translate('Path'),
    isVisible: true,
  },
  {
    name: 'monitored',
    label: () => translate('Monitored'),
    isVisible: true,
  },
];

function AuthorIndex() {
  const dispatch = useDispatch();
  const { isFetching, isPopulated, error, items } = useSelector(
    (state: AppState) => state.authors
  );

  useEffect(() => {
    dispatch(fetchAuthors());
  }, [dispatch]);

  const onRefreshPress = useCallback(() => {
    dispatch(fetchAuthors());
  }, [dispatch]);

  const hasNoAuthors = isPopulated && !items.length;

  return (
    <PageContent title={translate('Authors')}>
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
          <Alert kind={kinds.DANGER}>{translate('UnableToLoadAuthors')}</Alert>
        ) : null}

        {isPopulated && !error && items.length > 0 ? (
          <Table columns={columns}>
            <TableBody>
              {items.map((author) => (
                <AuthorIndexRow key={author.id} {...author} />
              ))}
            </TableBody>
          </Table>
        ) : null}

        {hasNoAuthors ? (
          <div style={{ padding: '20px', textAlign: 'center' }}>
            <p>{translate('NoAuthors')}</p>
            <p>Add authors to organize your book and audiobook library.</p>
          </div>
        ) : null}
      </PageContentBody>
    </PageContent>
  );
}

export default AuthorIndex;
