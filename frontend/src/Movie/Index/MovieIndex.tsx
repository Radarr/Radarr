import React, {
  useCallback,
  useEffect,
  useMemo,
  useRef,
  useState,
} from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { SelectProvider } from 'App/SelectContext';
import ClientSideCollectionAppState from 'App/State/ClientSideCollectionAppState';
import MoviesAppState, { MovieIndexAppState } from 'App/State/MoviesAppState';
import { RSS_SYNC } from 'Commands/commandNames';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import PageContent from 'Components/Page/PageContent';
import PageContentBody from 'Components/Page/PageContentBody';
import PageJumpBar from 'Components/Page/PageJumpBar';
import PageToolbar from 'Components/Page/Toolbar/PageToolbar';
import PageToolbarButton from 'Components/Page/Toolbar/PageToolbarButton';
import PageToolbarSection from 'Components/Page/Toolbar/PageToolbarSection';
import PageToolbarSeparator from 'Components/Page/Toolbar/PageToolbarSeparator';
import TableOptionsModalWrapper from 'Components/Table/TableOptions/TableOptionsModalWrapper';
import withScrollPosition from 'Components/withScrollPosition';
import { align, icons } from 'Helpers/Props';
import SortDirection from 'Helpers/Props/SortDirection';
import InteractiveImportModal from 'InteractiveImport/InteractiveImportModal';
import NoMovie from 'Movie/NoMovie';
import { executeCommand } from 'Store/Actions/commandActions';
import {
  setMovieFilter,
  setMovieSort,
  setMovieTableOption,
  setMovieView,
} from 'Store/Actions/movieIndexActions';
import { fetchQueueDetails } from 'Store/Actions/queueActions';
import scrollPositions from 'Store/scrollPositions';
import createCommandExecutingSelector from 'Store/Selectors/createCommandExecutingSelector';
import createDimensionsSelector from 'Store/Selectors/createDimensionsSelector';
import createMovieClientSideCollectionItemsSelector from 'Store/Selectors/createMovieClientSideCollectionItemsSelector';
import translate from 'Utilities/String/translate';
import MovieIndexFilterMenu from './Menus/MovieIndexFilterMenu';
import MovieIndexSortMenu from './Menus/MovieIndexSortMenu';
import MovieIndexViewMenu from './Menus/MovieIndexViewMenu';
import MovieIndexFooter from './MovieIndexFooter';
import MovieIndexRefreshMovieButton from './MovieIndexRefreshMovieButton';
import MovieIndexSearchButton from './MovieIndexSearchButton';
import MovieIndexOverviews from './Overview/MovieIndexOverviews';
import MovieIndexOverviewOptionsModal from './Overview/Options/MovieIndexOverviewOptionsModal';
import MovieIndexPosters from './Posters/MovieIndexPosters';
import MovieIndexPosterOptionsModal from './Posters/Options/MovieIndexPosterOptionsModal';
import MovieIndexSelectAllButton from './Select/MovieIndexSelectAllButton';
import MovieIndexSelectAllMenuItem from './Select/MovieIndexSelectAllMenuItem';
import MovieIndexSelectFooter from './Select/MovieIndexSelectFooter';
import MovieIndexSelectModeButton from './Select/MovieIndexSelectModeButton';
import MovieIndexSelectModeMenuItem from './Select/MovieIndexSelectModeMenuItem';
import MovieIndexTable from './Table/MovieIndexTable';
import MovieIndexTableOptions from './Table/MovieIndexTableOptions';
import styles from './MovieIndex.css';

function getViewComponent(view: string) {
  if (view === 'posters') {
    return MovieIndexPosters;
  }

  if (view === 'overview') {
    return MovieIndexOverviews;
  }

  return MovieIndexTable;
}

interface MovieIndexProps {
  initialScrollTop?: number;
}

const MovieIndex = withScrollPosition((props: MovieIndexProps) => {
  const {
    isFetching,
    isPopulated,
    error,
    totalItems,
    items,
    columns,
    selectedFilterKey,
    filters,
    customFilters,
    sortKey,
    sortDirection,
    view,
  }: MoviesAppState & MovieIndexAppState & ClientSideCollectionAppState =
    useSelector(createMovieClientSideCollectionItemsSelector('movieIndex'));

  const isRssSyncExecuting = useSelector(
    createCommandExecutingSelector(RSS_SYNC)
  );
  const { isSmallScreen } = useSelector(createDimensionsSelector());
  const dispatch = useDispatch();
  const scrollerRef = useRef<HTMLDivElement>(null);
  const [isOptionsModalOpen, setIsOptionsModalOpen] = useState(false);
  const [isInteractiveImportModalOpen, setIsInteractiveImportModalOpen] =
    useState(false);
  const [jumpToCharacter, setJumpToCharacter] = useState<string | undefined>(
    undefined
  );
  const [isSelectMode, setIsSelectMode] = useState(false);

  useEffect(() => {
    dispatch(fetchQueueDetails({ all: true }));
  }, [dispatch]);

  const onRssSyncPress = useCallback(() => {
    dispatch(
      executeCommand({
        name: RSS_SYNC,
      })
    );
  }, [dispatch]);

  const onSelectModePress = useCallback(() => {
    setIsSelectMode(!isSelectMode);
  }, [isSelectMode, setIsSelectMode]);

  const onTableOptionChange = useCallback(
    (payload: unknown) => {
      dispatch(setMovieTableOption(payload));
    },
    [dispatch]
  );

  const onViewSelect = useCallback(
    (value: string) => {
      dispatch(setMovieView({ view: value }));

      if (scrollerRef.current) {
        scrollerRef.current.scrollTo(0, 0);
      }
    },
    [scrollerRef, dispatch]
  );

  const onSortSelect = useCallback(
    (value: string) => {
      dispatch(setMovieSort({ sortKey: value }));
    },
    [dispatch]
  );

  const onFilterSelect = useCallback(
    (value: string) => {
      dispatch(setMovieFilter({ selectedFilterKey: value }));
    },
    [dispatch]
  );

  const onOptionsPress = useCallback(() => {
    setIsOptionsModalOpen(true);
  }, [setIsOptionsModalOpen]);

  const onOptionsModalClose = useCallback(() => {
    setIsOptionsModalOpen(false);
  }, [setIsOptionsModalOpen]);

  const onInteractiveImportPress = useCallback(() => {
    setIsInteractiveImportModalOpen(true);
  }, [setIsInteractiveImportModalOpen]);

  const onInteractiveImportModalClose = useCallback(() => {
    setIsInteractiveImportModalOpen(false);
  }, [setIsInteractiveImportModalOpen]);

  const onJumpBarItemPress = useCallback(
    (character: string) => {
      setJumpToCharacter(character);
    },
    [setJumpToCharacter]
  );

  const onScroll = useCallback(
    ({ scrollTop }: { scrollTop: number }) => {
      setJumpToCharacter(undefined);
      scrollPositions.movieIndex = scrollTop;
    },
    [setJumpToCharacter]
  );

  const jumpBarItems = useMemo(() => {
    // Reset if not sorting by sortTitle
    if (sortKey !== 'sortTitle') {
      return {
        order: [],
      };
    }

    const characters = items.reduce((acc: Record<string, number>, item) => {
      let char = item.sortTitle.charAt(0);

      if (!isNaN(Number(char))) {
        char = '#';
      }

      if (char in acc) {
        acc[char] = acc[char] + 1;
      } else {
        acc[char] = 1;
      }

      return acc;
    }, {});

    const order = Object.keys(characters).sort();

    // Reverse if sorting descending
    if (sortDirection === SortDirection.Descending) {
      order.reverse();
    }

    return {
      characters,
      order,
    };
  }, [items, sortKey, sortDirection]);
  const ViewComponent = useMemo(() => getViewComponent(view), [view]);

  const isLoaded = !!(!error && isPopulated && items.length);
  const hasNoMovie = !totalItems;

  return (
    <SelectProvider items={items}>
      <PageContent>
        <PageToolbar>
          <PageToolbarSection>
            <MovieIndexRefreshMovieButton
              isSelectMode={isSelectMode}
              selectedFilterKey={selectedFilterKey}
            />

            <PageToolbarButton
              label={translate('RSSSync')}
              iconName={icons.RSS}
              isSpinning={isRssSyncExecuting}
              isDisabled={hasNoMovie}
              onPress={onRssSyncPress}
            />

            <PageToolbarSeparator />

            <MovieIndexSearchButton
              isSelectMode={isSelectMode}
              selectedFilterKey={selectedFilterKey}
            />

            <PageToolbarButton
              label={translate('ManualImport')}
              iconName={icons.INTERACTIVE}
              isDisabled={hasNoMovie}
              onPress={onInteractiveImportPress}
            />

            <PageToolbarSeparator />

            <MovieIndexSelectModeButton
              label={
                isSelectMode
                  ? translate('StopSelecting')
                  : translate('EditMovies')
              }
              iconName={isSelectMode ? icons.SERIES_ENDED : icons.EDIT}
              isSelectMode={isSelectMode}
              overflowComponent={MovieIndexSelectModeMenuItem}
              onPress={onSelectModePress}
            />

            <MovieIndexSelectAllButton
              label="SelectAll"
              isSelectMode={isSelectMode}
              overflowComponent={MovieIndexSelectAllMenuItem}
            />
          </PageToolbarSection>

          <PageToolbarSection
            alignContent={align.RIGHT}
            collapseButtons={false}
          >
            {view === 'table' ? (
              <TableOptionsModalWrapper
                columns={columns}
                optionsComponent={MovieIndexTableOptions}
                onTableOptionChange={onTableOptionChange}
              >
                <PageToolbarButton
                  label={translate('Options')}
                  iconName={icons.TABLE}
                />
              </TableOptionsModalWrapper>
            ) : (
              <PageToolbarButton
                label={translate('Options')}
                iconName={view === 'posters' ? icons.POSTER : icons.OVERVIEW}
                isDisabled={hasNoMovie}
                onPress={onOptionsPress}
              />
            )}

            <PageToolbarSeparator />

            <MovieIndexViewMenu
              view={view}
              isDisabled={hasNoMovie}
              onViewSelect={onViewSelect}
            />

            <MovieIndexSortMenu
              sortKey={sortKey}
              sortDirection={sortDirection}
              isDisabled={hasNoMovie}
              onSortSelect={onSortSelect}
            />

            <MovieIndexFilterMenu
              selectedFilterKey={selectedFilterKey}
              filters={filters}
              customFilters={customFilters}
              isDisabled={hasNoMovie}
              onFilterSelect={onFilterSelect}
            />
          </PageToolbarSection>
        </PageToolbar>
        <div className={styles.pageContentBodyWrapper}>
          <PageContentBody
            ref={scrollerRef}
            className={styles.contentBody}
            // eslint-disable-next-line @typescript-eslint/ban-ts-comment
            // @ts-ignore
            innerClassName={styles[`${view}InnerContentBody`]}
            initialScrollTop={props.initialScrollTop}
            onScroll={onScroll}
          >
            {isFetching && !isPopulated ? <LoadingIndicator /> : null}

            {!isFetching && !!error ? (
              <div>{translate('UnableToLoadMovies')}</div>
            ) : null}

            {isLoaded ? (
              <div className={styles.contentBodyContainer}>
                <ViewComponent
                  scrollerRef={scrollerRef}
                  items={items}
                  sortKey={sortKey}
                  sortDirection={sortDirection}
                  jumpToCharacter={jumpToCharacter}
                  isSelectMode={isSelectMode}
                  isSmallScreen={isSmallScreen}
                />

                <MovieIndexFooter />
              </div>
            ) : null}

            {!error && isPopulated && !items.length ? (
              <NoMovie totalItems={totalItems} />
            ) : null}
          </PageContentBody>

          {isLoaded && !!jumpBarItems.order.length ? (
            <PageJumpBar
              items={jumpBarItems}
              onItemPress={onJumpBarItemPress}
            />
          ) : null}
        </div>

        {isSelectMode ? <MovieIndexSelectFooter /> : null}

        <InteractiveImportModal
          isOpen={isInteractiveImportModalOpen}
          onModalClose={onInteractiveImportModalClose}
        />

        {view === 'posters' ? (
          <MovieIndexPosterOptionsModal
            isOpen={isOptionsModalOpen}
            onModalClose={onOptionsModalClose}
          />
        ) : null}
        {view === 'overview' ? (
          <MovieIndexOverviewOptionsModal
            isOpen={isOptionsModalOpen}
            onModalClose={onOptionsModalClose}
          />
        ) : null}
      </PageContent>
    </SelectProvider>
  );
}, 'movieIndex');

export default MovieIndex;
