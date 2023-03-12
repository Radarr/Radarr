import React, { useCallback, useMemo, useRef, useState } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { REFRESH_MOVIE, RSS_SYNC } from 'Commands/commandNames';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import PageContent from 'Components/Page/PageContent';
import PageContentBody from 'Components/Page/PageContentBody';
import PageJumpBar from 'Components/Page/PageJumpBar';
import PageToolbar from 'Components/Page/Toolbar/PageToolbar';
import PageToolbarButton from 'Components/Page/Toolbar/PageToolbarButton';
import PageToolbarSection from 'Components/Page/Toolbar/PageToolbarSection';
import PageToolbarSeparator from 'Components/Page/Toolbar/PageToolbarSeparator';
import TableOptionsModalWrapper from 'Components/Table/TableOptions/TableOptionsModalWrapper';
import { align, icons } from 'Helpers/Props';
import SortDirection from 'Helpers/Props/SortDirection';
import NoMovie from 'Movie/NoMovie';
import { executeCommand } from 'Store/Actions/commandActions';
import {
  setMovieFilter,
  setMovieSort,
  setMovieTableOption,
  setMovieView,
} from 'Store/Actions/movieIndexActions';
import scrollPositions from 'Store/scrollPositions';
import createCommandExecutingSelector from 'Store/Selectors/createCommandExecutingSelector';
import createDimensionsSelector from 'Store/Selectors/createDimensionsSelector';
import createMovieClientSideCollectionItemsSelector from 'Store/Selectors/createMovieClientSideCollectionItemsSelector';
import MovieIndexFilterMenu from './Menus/MovieIndexFilterMenu';
import MovieIndexSortMenu from './Menus/MovieIndexSortMenu';
import MovieIndexViewMenu from './Menus/MovieIndexViewMenu';
import MovieIndexFooter from './MovieIndexFooter';
import MovieIndexOverviews from './Overview/MovieIndexOverviews';
import MovieIndexOverviewOptionsModal from './Overview/Options/MovieIndexOverviewOptionsModal';
import MovieIndexPosters from './Posters/MovieIndexPosters';
import MovieIndexPosterOptionsModal from './Posters/Options/MovieIndexPosterOptionsModal';
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

function MovieIndex() {
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
  } = useSelector(createMovieClientSideCollectionItemsSelector('movieIndex'));

  const isRefreshingMovie = useSelector(
    createCommandExecutingSelector(REFRESH_MOVIE)
  );
  const isRssSyncExecuting = useSelector(
    createCommandExecutingSelector(RSS_SYNC)
  );
  const { isSmallScreen } = useSelector(createDimensionsSelector());
  const dispatch = useDispatch();
  const scrollerRef = useRef<HTMLDivElement>();
  const [isOptionsModalOpen, setIsOptionsModalOpen] = useState(false);
  const [jumpToCharacter, setJumpToCharacter] = useState<string | null>(null);

  const onRefreshMoviePress = useCallback(() => {
    dispatch(
      executeCommand({
        name: REFRESH_MOVIE,
      })
    );
  }, [dispatch]);

  const onRssSyncPress = useCallback(() => {
    dispatch(
      executeCommand({
        name: RSS_SYNC,
      })
    );
  }, [dispatch]);

  const onTableOptionChange = useCallback(
    (payload) => {
      dispatch(setMovieTableOption(payload));
    },
    [dispatch]
  );

  const onViewSelect = useCallback(
    (value) => {
      dispatch(setMovieView({ view: value }));

      if (scrollerRef.current) {
        scrollerRef.current.scrollTo(0, 0);
      }
    },
    [scrollerRef, dispatch]
  );

  const onSortSelect = useCallback(
    (value) => {
      dispatch(setMovieSort({ sortKey: value }));
    },
    [dispatch]
  );

  const onFilterSelect = useCallback(
    (value) => {
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

  const onJumpBarItemPress = useCallback(
    (character) => {
      setJumpToCharacter(character);
    },
    [setJumpToCharacter]
  );

  const onScroll = useCallback(
    ({ scrollTop }) => {
      setJumpToCharacter(null);
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

    const characters = items.reduce((acc, item) => {
      let char = item.sortTitle.charAt(0);

      if (!isNaN(char)) {
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
    <PageContent>
      <PageToolbar>
        <PageToolbarSection>
          <PageToolbarButton
            label="Update all"
            iconName={icons.REFRESH}
            spinningName={icons.REFRESH}
            isSpinning={isRefreshingMovie}
            isDisabled={hasNoMovie}
            onPress={onRefreshMoviePress}
          />

          <PageToolbarButton
            label="RSS Sync"
            iconName={icons.RSS}
            isSpinning={isRssSyncExecuting}
            isDisabled={hasNoMovie}
            onPress={onRssSyncPress}
          />
        </PageToolbarSection>

        <PageToolbarSection alignContent={align.RIGHT} collapseButtons={false}>
          {view === 'table' ? (
            <TableOptionsModalWrapper
              columns={columns}
              optionsComponent={MovieIndexTableOptions}
              onTableOptionChange={onTableOptionChange}
            >
              <PageToolbarButton label="Options" iconName={icons.TABLE} />
            </TableOptionsModalWrapper>
          ) : (
            <PageToolbarButton
              label="Options"
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
          innerClassName={styles[`${view}InnerContentBody`]}
          onScroll={onScroll}
        >
          {isFetching && !isPopulated ? <LoadingIndicator /> : null}

          {!isFetching && !!error ? <div>Unable to load movie</div> : null}

          {isLoaded ? (
            <div className={styles.contentBodyContainer}>
              <ViewComponent
                scrollerRef={scrollerRef}
                items={items}
                sortKey={sortKey}
                sortDirection={sortDirection}
                jumpToCharacter={jumpToCharacter}
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
          <PageJumpBar items={jumpBarItems} onItemPress={onJumpBarItemPress} />
        ) : null}
      </div>
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
  );
}

export default MovieIndex;
