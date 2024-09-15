import React from 'react';
import { useSelector } from 'react-redux';
import { createSelector } from 'reselect';
import AppState from 'App/State/AppState';
import Alert from 'Components/Alert';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import Column from 'Components/Table/Column';
import Table from 'Components/Table/Table';
import TableBody from 'Components/Table/TableBody';
import { kinds } from 'Helpers/Props';
import sortByProp from 'Utilities/Array/sortByProp';
import translate from 'Utilities/String/translate';
import MovieTitlesRow from './MovieTitlesRow';
import styles from './MovieTitlesTable.css';

const columns: Column[] = [
  {
    name: 'alternativeTitle',
    label: () => translate('AlternativeTitle'),
    isVisible: true,
  },
  {
    name: 'sourceType',
    label: () => translate('Type'),
    isVisible: true,
  },
];

function movieAlternativeTitlesSelector(movieId: number) {
  return createSelector(
    (state: AppState) => state.movies,
    (movies) => {
      const { isFetching, isPopulated, error, items } = movies;

      const alternateTitles =
        items.find((m) => m.id === movieId)?.alternateTitles ?? [];

      return {
        isFetching,
        isPopulated,
        error,
        items: alternateTitles,
      };
    }
  );
}

interface MovieTitlesProps {
  movieId: number;
}

function MovieTitlesTable({ movieId }: MovieTitlesProps) {
  const { isFetching, isPopulated, error, items } = useSelector(
    movieAlternativeTitlesSelector(movieId)
  );

  const sortedItems = items.sort(sortByProp('title'));

  if (!isFetching && !!error) {
    return (
      <Alert kind={kinds.DANGER}>
        {translate('AlternativeTitlesLoadError')}
      </Alert>
    );
  }

  return (
    <div className={styles.container}>
      {isFetching && <LoadingIndicator />}

      {isPopulated && !items.length && !error ? (
        <div className={styles.blankpad}>
          {translate('NoAlternativeTitles')}
        </div>
      ) : null}

      {isPopulated && !!items.length && !error ? (
        <Table columns={columns}>
          <TableBody>
            {sortedItems.map((item) => (
              <MovieTitlesRow
                key={item.id}
                title={item.title}
                sourceType={item.sourceType}
              />
            ))}
          </TableBody>
        </Table>
      ) : null}
    </div>
  );
}

export default MovieTitlesTable;
