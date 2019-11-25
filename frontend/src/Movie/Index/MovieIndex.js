import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import hasDifferentItemsOrOrder from 'Utilities/Object/hasDifferentItemsOrOrder';
import getSelectedIds from 'Utilities/Table/getSelectedIds';
import selectAll from 'Utilities/Table/selectAll';
import toggleSelected from 'Utilities/Table/toggleSelected';
import { align, icons, kinds, sortDirections } from 'Helpers/Props';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import PageContent from 'Components/Page/PageContent';
import PageContentBodyConnector from 'Components/Page/PageContentBodyConnector';
import PageJumpBar from 'Components/Page/PageJumpBar';
import TableOptionsModalWrapper from 'Components/Table/TableOptions/TableOptionsModalWrapper';
import PageToolbar from 'Components/Page/Toolbar/PageToolbar';
import PageToolbarSeparator from 'Components/Page/Toolbar/PageToolbarSeparator';
import PageToolbarSection from 'Components/Page/Toolbar/PageToolbarSection';
import PageToolbarButton from 'Components/Page/Toolbar/PageToolbarButton';
import ConfirmModal from 'Components/Modal/ConfirmModal';
import NoMovie from 'Movie/NoMovie';
import MovieIndexTableConnector from './Table/MovieIndexTableConnector';
import MovieIndexTableOptionsConnector from './Table/MovieIndexTableOptionsConnector';
import MovieIndexPosterOptionsModal from './Posters/Options/MovieIndexPosterOptionsModal';
import MovieIndexPostersConnector from './Posters/MovieIndexPostersConnector';
import MovieIndexOverviewOptionsModal from './Overview/Options/MovieIndexOverviewOptionsModal';
import MovieIndexOverviewsConnector from './Overview/MovieIndexOverviewsConnector';
import MovieIndexFilterMenu from './Menus/MovieIndexFilterMenu';
import MovieIndexSortMenu from './Menus/MovieIndexSortMenu';
import MovieIndexSearchMenu from './Menus/MovieIndexSearchMenu';
import MovieIndexViewMenu from './Menus/MovieIndexViewMenu';
import MovieIndexFooterConnector from './MovieIndexFooterConnector';
import MovieEditorFooter from 'Movie/Editor/MovieEditorFooter.js';
import InteractiveImportModal from 'InteractiveImport/InteractiveImportModal';
import OrganizeMovieModal from 'Movie/Editor/Organize/OrganizeMovieModal';
import styles from './MovieIndex.css';

function getViewComponent(view) {
  if (view === 'posters') {
    return MovieIndexPostersConnector;
  }

  if (view === 'overview') {
    return MovieIndexOverviewsConnector;
  }

  return MovieIndexTableConnector;
}

class MovieIndex extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      contentBody: null,
      jumpBarItems: [],
      jumpToCharacter: null,
      isPosterOptionsModalOpen: false,
      isOverviewOptionsModalOpen: false,
      isInteractiveImportModalOpen: false,
      isMovieEditorActive: false,
      isOrganizingMovieModalOpen: false,
      isConfirmSearchModalOpen: false,
      searchType: null,
      allSelected: false,
      allUnselected: false,
      lastToggled: null,
      selectedState: {},
      isRendered: false
    };
  }

  componentDidMount() {
    this.setJumpBarItems();
    this.setSelectedState();
  }

  componentDidUpdate(prevProps) {
    const {
      items,
      sortKey,
      sortDirection,
      scrollTop,
      isDeleting,
      deleteError
    } = this.props;

    if (sortKey !== prevProps.sortKey ||
        sortDirection !== prevProps.sortDirection ||
        hasDifferentItemsOrOrder(prevProps.items, items)
    ) {
      this.setJumpBarItems();
      this.setSelectedState();
    }

    if (this.state.jumpToCharacter != null && scrollTop !== prevProps.scrollTop) {
      this.setState({ jumpToCharacter: null });
    }

    const hasFinishedDeleting = prevProps.isDeleting &&
                                !isDeleting &&
                                !deleteError;

    if (hasFinishedDeleting) {
      this.onSelectAllChange({ value: false });
    }
  }

  //
  // Control

  setContentBodyRef = (ref) => {
    this.setState({ contentBody: ref });
  }

  getSelectedIds = () => {
    if (this.state.allUnselected) {
      return [];
    }
    return getSelectedIds(this.state.selectedState);
  }

  setSelectedState() {
    const {
      items
    } = this.props;

    const {
      selectedState
    } = this.state;

    const newSelectedState = {};

    items.forEach((movie) => {
      const isItemSelected = selectedState[movie.id];

      if (isItemSelected) {
        newSelectedState[movie.id] = isItemSelected;
      } else {
        newSelectedState[movie.id] = false;
      }
    });

    const selectedCount = getSelectedIds(newSelectedState).length;
    const newStateCount = Object.keys(newSelectedState).length;
    let isAllSelected = false;
    let isAllUnselected = false;

    if (selectedCount === 0) {
      isAllUnselected = true;
    } else if (selectedCount === newStateCount) {
      isAllSelected = true;
    }

    this.setState({ selectedState: newSelectedState, allSelected: isAllSelected, allUnselected: isAllUnselected });
  }

  setJumpBarItems() {
    const {
      items,
      sortKey,
      sortDirection
    } = this.props;

    // Reset if not sorting by sortTitle
    if (sortKey !== 'sortTitle') {
      this.setState({ jumpBarItems: [] });
      return;
    }

    const characters = _.reduce(items, (acc, item) => {
      const firstCharacter = item.sortTitle.charAt(0);

      if (isNaN(firstCharacter)) {
        acc.push(firstCharacter);
      } else {
        acc.push('#');
      }

      return acc;
    }, []).sort();

    // Reverse if sorting descending
    if (sortDirection === sortDirections.DESCENDING) {
      characters.reverse();
    }

    this.setState({ jumpBarItems: _.sortedUniq(characters) });
  }

  //
  // Listeners

  onPosterOptionsPress = () => {
    this.setState({ isPosterOptionsModalOpen: true });
  }

  onPosterOptionsModalClose = () => {
    this.setState({ isPosterOptionsModalOpen: false });
  }

  onOverviewOptionsPress = () => {
    this.setState({ isOverviewOptionsModalOpen: true });
  }

  onOverviewOptionsModalClose = () => {
    this.setState({ isOverviewOptionsModalOpen: false });
  }

  onInteractiveImportPress = () => {
    this.setState({ isInteractiveImportModalOpen: true });
  }

  onInteractiveImportModalClose = () => {
    this.setState({ isInteractiveImportModalOpen: false });
  }

  onMovieEditorTogglePress = () => {
    if (this.state.isMovieEditorActive) {
      this.setState({ isMovieEditorActive: false });
    } else {
      const newState = selectAll(this.state.selectedState, false);
      newState.isMovieEditorActive = true;
      this.setState(newState);
    }
  }

  onJumpBarItemPress = (jumpToCharacter) => {
    this.setState({ jumpToCharacter });
  }

  onSelectAllChange = ({ value }) => {
    this.setState(selectAll(this.state.selectedState, value));
  }

  onSelectAllPress = () => {
    this.onSelectAllChange({ value: !this.state.allSelected });
  }

  onSelectedChange = ({ id, value, shiftKey = false }) => {
    this.setState((state) => {
      return toggleSelected(state, this.props.items, id, value, shiftKey);
    });
  }

  onSaveSelected = (changes) => {
    this.props.onSaveSelected({
      movieIds: this.getSelectedIds(),
      ...changes
    });
  }

  onOrganizeMoviePress = () => {
    this.setState({ isOrganizingMovieModalOpen: true });
  }

  onOrganizeMovieModalClose = (organized) => {
    this.setState({ isOrganizingMovieModalOpen: false });

    if (organized === true) {
      this.onSelectAllChange({ value: false });
    }
  }

  onSearchPress = (command) => {
    this.setState({ isConfirmSearchModalOpen: true, searchType: command });
  }

  onSearchConfirmed = () => {
    this.props.onSearchPress(this.state.searchType);
    this.setState({ isConfirmSearchModalOpen: false });
  }

  onConfirmSearchModalClose = () => {
    this.setState({ isConfirmSearchModalOpen: false });
  }

  onRender = () => {
    this.setState({ isRendered: true }, () => {
      const {
        scrollTop,
        isSmallScreen
      } = this.props;

      if (isSmallScreen) {
        // Seems to result in the view being off by 125px (distance to the top of the page)
        // document.documentElement.scrollTop = document.body.scrollTop = scrollTop;

        // This works, but then jumps another 1px after scrolling
        document.documentElement.scrollTop = scrollTop;
      }
    });
  }

  onScroll = ({ scrollTop }) => {
    this.props.onScroll({ scrollTop });
  }

  //
  // Render

  render() {
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
      isRefreshingMovie,
      isRssSyncExecuting,
      isOrganizingMovie,
      isSearchingMovies,
      isSaving,
      saveError,
      isDeleting,
      deleteError,
      scrollTop,
      onSortSelect,
      onFilterSelect,
      onViewSelect,
      onRefreshMoviePress,
      onRssSyncPress,
      onSearchPress,
      ...otherProps
    } = this.props;

    const {
      contentBody,
      jumpBarItems,
      jumpToCharacter,
      isPosterOptionsModalOpen,
      isOverviewOptionsModalOpen,
      isInteractiveImportModalOpen,
      isConfirmSearchModalOpen,
      isMovieEditorActive,
      isRendered,
      selectedState,
      allSelected,
      allUnselected
    } = this.state;

    const selectedMovieIds = this.getSelectedIds();

    const ViewComponent = getViewComponent(view);
    const isLoaded = !!(!error && isPopulated && items.length && contentBody);
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

            <PageToolbarSeparator />

            <MovieIndexSearchMenu
              isDisabled={isSearchingMovies}
              onSearchPress={this.onSearchPress}
            />

            <PageToolbarButton
              label="Manual Import"
              iconName={icons.INTERACTIVE}
              isDisabled={hasNoMovie}
              onPress={this.onInteractiveImportPress}
            />

            <PageToolbarSeparator />

            {
              isMovieEditorActive ?
                <PageToolbarButton
                  label="Movie Index"
                  iconName={icons.MOVIE_CONTINUING}
                  isDisabled={hasNoMovie}
                  onPress={this.onMovieEditorTogglePress}
                /> :
                <PageToolbarButton
                  label="Movie Editor"
                  iconName={icons.EDIT}
                  isDisabled={hasNoMovie}
                  onPress={this.onMovieEditorTogglePress}
                />
            }

            {
              isMovieEditorActive ?
                <PageToolbarButton
                  label={allSelected ? 'Unselect All' : 'Select All'}
                  iconName={icons.CHECK_SQUARE}
                  isDisabled={hasNoMovie}
                  onPress={this.onSelectAllPress}
                /> :
                null
            }

          </PageToolbarSection>

          <PageToolbarSection
            alignContent={align.RIGHT}
            collapseButtons={false}
          >
            {
              view === 'table' ?
                <TableOptionsModalWrapper
                  {...otherProps}
                  columns={columns}
                  optionsComponent={MovieIndexTableOptionsConnector}
                >
                  <PageToolbarButton
                    label="Options"
                    iconName={icons.TABLE}
                  />
                </TableOptionsModalWrapper> :
                null
            }

            {
              view === 'posters' ?
                <PageToolbarButton
                  label="Options"
                  iconName={icons.POSTER}
                  isDisabled={hasNoMovie}
                  onPress={this.onPosterOptionsPress}
                /> :
                null
            }

            {
              view === 'overview' ?
                <PageToolbarButton
                  label="Options"
                  iconName={icons.OVERVIEW}
                  isDisabled={hasNoMovie}
                  onPress={this.onOverviewOptionsPress}
                /> :
                null
            }

            {
              (view === 'posters' || view === 'overview') &&
                <PageToolbarSeparator />
            }

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
          <PageContentBodyConnector
            ref={this.setContentBodyRef}
            className={styles.contentBody}
            innerClassName={styles[`${view}InnerContentBody`]}
            scrollTop={isRendered ? scrollTop : 0}
            onScroll={this.onScroll}
          >
            {
              isFetching && !isPopulated &&
                <LoadingIndicator />
            }

            {
              !isFetching && !!error &&
                <div>Unable to load movies</div>
            }

            {
              isLoaded &&
                <div className={styles.contentBodyContainer}>
                  <ViewComponent
                    contentBody={contentBody}
                    items={items}
                    filters={filters}
                    sortKey={sortKey}
                    sortDirection={sortDirection}
                    scrollTop={scrollTop}
                    jumpToCharacter={jumpToCharacter}
                    onRender={this.onRender}
                    isMovieEditorActive={isMovieEditorActive}
                    allSelected={allSelected}
                    allUnselected={allUnselected}
                    onSelectedChange={this.onSelectedChange}
                    onSelectAllChange={this.onSelectAllChange}
                    selectedState={selectedState}
                    {...otherProps}
                  />

                  {
                    !isMovieEditorActive &&
                      <MovieIndexFooterConnector />
                  }
                </div>
            }

            {
              !error && isPopulated && !items.length &&
                <NoMovie totalItems={totalItems} />
            }
          </PageContentBodyConnector>

          {
            isLoaded && !!jumpBarItems.length &&
              <PageJumpBar
                items={jumpBarItems}
                onItemPress={this.onJumpBarItemPress}
              />
          }
        </div>

        {
          isLoaded && isMovieEditorActive &&
            <MovieEditorFooter
              movieIds={selectedMovieIds}
              selectedCount={selectedMovieIds.length}
              isSaving={isSaving}
              saveError={saveError}
              isDeleting={isDeleting}
              deleteError={deleteError}
              isOrganizingMovie={isOrganizingMovie}
              onSaveSelected={this.onSaveSelected}
              onOrganizeMoviePress={this.onOrganizeMoviePress}
            />
        }

        <MovieIndexPosterOptionsModal
          isOpen={isPosterOptionsModalOpen}
          onModalClose={this.onPosterOptionsModalClose}
        />

        <MovieIndexOverviewOptionsModal
          isOpen={isOverviewOptionsModalOpen}
          onModalClose={this.onOverviewOptionsModalClose}
        />

        <InteractiveImportModal
          isOpen={isInteractiveImportModalOpen}
          onModalClose={this.onInteractiveImportModalClose}
        />

        <OrganizeMovieModal
          isOpen={this.state.isOrganizingMovieModalOpen}
          movieIds={selectedMovieIds}
          onModalClose={this.onOrganizeMovieModalClose}
        />

        <ConfirmModal
          isOpen={isConfirmSearchModalOpen}
          kind={kinds.DANGER}
          title="Mass Movie Search"
          message={
            <div>
              <div>
                Are you sure you want to perform mass movie search?
              </div>
              <div>
                This cannot be cancelled once started without restarting Radarr.
              </div>
            </div>
          }
          confirmLabel="Search"
          onConfirm={this.onSearchConfirmed}
          onCancel={this.onConfirmSearchModalClose}
        />
      </PageContent>
    );
  }
}

MovieIndex.propTypes = {
  isFetching: PropTypes.bool.isRequired,
  isPopulated: PropTypes.bool.isRequired,
  error: PropTypes.object,
  totalItems: PropTypes.number.isRequired,
  items: PropTypes.arrayOf(PropTypes.object).isRequired,
  columns: PropTypes.arrayOf(PropTypes.object).isRequired,
  selectedFilterKey: PropTypes.oneOfType([PropTypes.string, PropTypes.number]).isRequired,
  filters: PropTypes.arrayOf(PropTypes.object).isRequired,
  customFilters: PropTypes.arrayOf(PropTypes.object).isRequired,
  sortKey: PropTypes.string,
  sortDirection: PropTypes.oneOf(sortDirections.all),
  view: PropTypes.string.isRequired,
  isRefreshingMovie: PropTypes.bool.isRequired,
  isOrganizingMovie: PropTypes.bool.isRequired,
  isSearchingMovies: PropTypes.bool.isRequired,
  isRssSyncExecuting: PropTypes.bool.isRequired,
  scrollTop: PropTypes.number.isRequired,
  isSmallScreen: PropTypes.bool.isRequired,
  isSaving: PropTypes.bool.isRequired,
  saveError: PropTypes.object,
  isDeleting: PropTypes.bool.isRequired,
  deleteError: PropTypes.object,
  onSortSelect: PropTypes.func.isRequired,
  onFilterSelect: PropTypes.func.isRequired,
  onViewSelect: PropTypes.func.isRequired,
  onRefreshMoviePress: PropTypes.func.isRequired,
  onRssSyncPress: PropTypes.func.isRequired,
  onSearchPress: PropTypes.func.isRequired,
  onScroll: PropTypes.func.isRequired,
  onSaveSelected: PropTypes.func.isRequired
};

export default MovieIndex;
