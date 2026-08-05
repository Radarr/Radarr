import React, { useCallback, useEffect } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import ClientSideCollectionAppState from 'App/State/ClientSideCollectionAppState';
import ReleasesAppState from 'App/State/ReleasesAppState';
import Alert from 'Components/Alert';
import Icon from 'Components/Icon';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import FilterMenu from 'Components/Menu/FilterMenu';
import PageMenuButton from 'Components/Menu/PageMenuButton';
import Column from 'Components/Table/Column';
import Table from 'Components/Table/Table';
import TableBody from 'Components/Table/TableBody';
import { align, icons, kinds, sortDirections } from 'Helpers/Props';
import { SortDirection } from 'Helpers/Props/sortDirections';
import { fetchMovieBlocklist } from 'Store/Actions/movieBlocklistActions';
import { fetchMovieHistory } from 'Store/Actions/movieHistoryActions';
import {
  fetchReleases,
  grabRelease,
  setReleasesFilter,
  setReleasesSort,
} from 'Store/Actions/releaseActions';
import createClientSideCollectionSelector from 'Store/Selectors/createClientSideCollectionSelector';
import getErrorMessage from 'Utilities/Object/getErrorMessage';
import translate from 'Utilities/String/translate';
import InteractiveSearchFilterModal from './InteractiveSearchFilterModal';
import InteractiveSearchPayload from './InteractiveSearchPayload';
import InteractiveSearchRow from './InteractiveSearchRow';
import styles from './InteractiveSearch.css';

const columns: Column[] = [
  {
    name: 'protocol',
    label: () => translate('Source'),
    isSortable: true,
    isVisible: true,
  },
  {
    name: 'age',
    label: () => translate('Age'),
    isSortable: true,
    isVisible: true,
  },
  {
    name: 'title',
    label: () => translate('Title'),
    isSortable: true,
    isVisible: true,
  },
  {
    name: 'indexer',
    label: () => translate('Indexer'),
    isSortable: true,
    isVisible: true,
  },
  {
    name: 'history',
    label: () => translate('History'),
    isSortable: true,
    fixedSortDirection: sortDirections.ASCENDING,
    isVisible: true,
  },
  {
    name: 'size',
    label: () => translate('Size'),
    isSortable: true,
    isVisible: true,
  },
  {
    name: 'peers',
    label: () => translate('Peers'),
    isSortable: true,
    isVisible: true,
  },
  {
    name: 'languages',
    label: () => translate('Language'),
    isSortable: true,
    isVisible: true,
  },
  {
    name: 'qualityWeight',
    label: () => translate('Quality'),
    isSortable: true,
    isVisible: true,
  },
  {
    name: 'customFormatScore',
    label: React.createElement(Icon, {
      name: icons.SCORE,
      title: () => translate('CustomFormatScore'),
    }),
    isSortable: true,
    isVisible: true,
  },
  {
    name: 'indexerFlags',
    label: React.createElement(Icon, {
      name: icons.FLAG,
      title: () => translate('IndexerFlags'),
    }),
    isSortable: true,
    isVisible: true,
  },
  {
    name: 'rejections',
    label: React.createElement(Icon, {
      name: icons.DANGER,
      title: () => translate('Rejections'),
    }),
    isSortable: true,
    fixedSortDirection: sortDirections.ASCENDING,
    isVisible: true,
  },
  {
    name: 'releaseWeight',
    label: React.createElement(Icon, { name: icons.DOWNLOAD }),
    isSortable: true,
    fixedSortDirection: sortDirections.ASCENDING,
    isVisible: true,
  },
];

const reversibleSortDirections: SortDirection[] = [
  sortDirections.ASCENDING,
  sortDirections.DESCENDING,
];

interface MobileSortOption {
  name: string;
  label: () => string;
  directions: SortDirection[];
}

const mobileSortOptions: MobileSortOption[] = [
  {
    name: 'protocol',
    label: () => translate('Source'),
    directions: reversibleSortDirections,
  },
  {
    name: 'age',
    label: () => translate('Age'),
    directions: reversibleSortDirections,
  },
  {
    name: 'title',
    label: () => translate('Title'),
    directions: reversibleSortDirections,
  },
  {
    name: 'indexer',
    label: () => translate('Indexer'),
    directions: reversibleSortDirections,
  },
  {
    name: 'history',
    label: () => translate('History'),
    directions: [sortDirections.ASCENDING],
  },
  {
    name: 'size',
    label: () => translate('Size'),
    directions: reversibleSortDirections,
  },
  {
    name: 'peers',
    label: () => translate('Peers'),
    directions: reversibleSortDirections,
  },
  {
    name: 'languages',
    label: () => translate('Language'),
    directions: reversibleSortDirections,
  },
  {
    name: 'qualityWeight',
    label: () => translate('Quality'),
    directions: reversibleSortDirections,
  },
  {
    name: 'customFormatScore',
    label: () => translate('CustomFormatScore'),
    directions: reversibleSortDirections,
  },
  {
    name: 'indexerFlags',
    label: () => translate('IndexerFlags'),
    directions: reversibleSortDirections,
  },
  {
    name: 'rejections',
    label: () => translate('Rejections'),
    directions: [sortDirections.ASCENDING],
  },
  {
    name: 'releaseWeight',
    label: () => translate('AddToDownloadQueue'),
    directions: [sortDirections.ASCENDING],
  },
];

interface InteractiveSearchProps {
  searchPayload: InteractiveSearchPayload;
}

function InteractiveSearch({ searchPayload }: InteractiveSearchProps) {
  const {
    isFetching,
    isPopulated,
    error,
    items,
    totalItems,
    selectedFilterKey,
    filters,
    customFilters,
    sortKey,
    sortDirection,
  }: ReleasesAppState & ClientSideCollectionAppState = useSelector(
    createClientSideCollectionSelector('releases')
  );

  const dispatch = useDispatch();

  const handleFilterSelect = useCallback(
    (selectedFilterKey: string | number) => {
      dispatch(setReleasesFilter({ selectedFilterKey }));
    },
    [dispatch]
  );

  const handleSortPress = useCallback(
    (sortKey: string, sortDirection?: SortDirection) => {
      dispatch(setReleasesSort({ sortKey, sortDirection }));
    },
    [dispatch]
  );

  const handleMobileSortChange = useCallback(
    ({ currentTarget }: React.ChangeEvent<HTMLSelectElement>) => {
      const [nextSortKey, nextSortDirection] = currentTarget.value.split('|');

      handleSortPress(nextSortKey, nextSortDirection as SortDirection);
    },
    [handleSortPress]
  );

  const handleGrabPress = useCallback(
    (payload: object) => {
      dispatch(grabRelease(payload));
    },
    [dispatch]
  );

  useEffect(
    () => {
      // Only fetch releases if they are not already being fetched and not yet populated.

      if (!isFetching && !isPopulated) {
        dispatch(fetchReleases(searchPayload));

        const { movieId } = searchPayload;

        if (movieId) {
          dispatch(fetchMovieBlocklist({ movieId }));
          dispatch(fetchMovieHistory({ movieId }));
        }
      }
    },
    // eslint-disable-next-line react-hooks/exhaustive-deps
    []
  );

  const errorMessage = getErrorMessage(error);

  return (
    <div>
      <div className={styles.filterMenuContainer}>
        <FilterMenu
          alignMenu={align.RIGHT}
          selectedFilterKey={selectedFilterKey}
          filters={filters}
          customFilters={customFilters}
          buttonComponent={PageMenuButton}
          filterModalConnectorComponent={InteractiveSearchFilterModal}
          filterModalConnectorComponentProps={{ type: 'movies' }}
          onFilterSelect={handleFilterSelect}
        />
      </div>

      <div className={styles.mobileSort}>
        <label
          className={styles.mobileSortLabel}
          htmlFor="interactive-search-sort"
        >
          {translate('Sort')}
        </label>
        <select
          id="interactive-search-sort"
          className={styles.mobileSortSelect}
          value={`${sortKey}|${sortDirection || sortDirections.ASCENDING}`}
          onChange={handleMobileSortChange}
        >
          {mobileSortOptions.flatMap((option) => {
            const label = option.label();

            return option.directions.map((direction) => {
              const directionIndicator =
                direction === sortDirections.ASCENDING ? '↑' : '↓';

              return (
                <option
                  key={`${option.name}-${direction}`}
                  value={`${option.name}|${direction}`}
                >
                  {label} {directionIndicator}
                </option>
              );
            });
          })}
        </select>
      </div>

      {isFetching ? <LoadingIndicator /> : null}

      {!isFetching && error ? (
        <Alert kind={kinds.DANGER} className={styles.alert}>
          {errorMessage ? (
            <>
              {translate('InteractiveSearchResultsFailedErrorMessage', {
                message:
                  errorMessage.charAt(0).toLowerCase() + errorMessage.slice(1),
              })}
            </>
          ) : (
            translate('MovieSearchResultsLoadError')
          )}
        </Alert>
      ) : null}

      {!isFetching && isPopulated && !totalItems ? (
        <Alert kind={kinds.INFO} className={styles.alert}>
          {translate('NoResultsFound')}
        </Alert>
      ) : null}

      {!!totalItems && isPopulated && !items.length ? (
        <Alert kind={kinds.WARNING} className={styles.alert}>
          {translate('AllResultsHiddenFilter')}
        </Alert>
      ) : null}

      {isPopulated && !!items.length ? (
        <div className={styles.results}>
          <Table
            columns={columns}
            sortKey={sortKey}
            sortDirection={sortDirection}
            onSortPress={handleSortPress}
          >
            <TableBody>
              {items.map((item) => {
                return (
                  <InteractiveSearchRow
                    key={`${item.indexerId}-${item.guid}`}
                    {...item}
                    searchPayload={searchPayload}
                    onGrabPress={handleGrabPress}
                  />
                );
              })}
            </TableBody>
          </Table>
        </div>
      ) : null}

      {totalItems !== items.length && !!items.length ? (
        <Alert kind={kinds.INFO} className={styles.alert}>
          {translate('SomeResultsHiddenFilter')}
        </Alert>
      ) : null}
    </div>
  );
}

export default InteractiveSearch;
