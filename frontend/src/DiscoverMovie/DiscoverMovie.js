import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import PageContent from 'Components/Page/PageContent';
import PageContentBody from 'Components/Page/PageContentBody';
import PageJumpBar from 'Components/Page/PageJumpBar';
import PageToolbar from 'Components/Page/Toolbar/PageToolbar';
import PageToolbarButton from 'Components/Page/Toolbar/PageToolbarButton';
import PageToolbarSection from 'Components/Page/Toolbar/PageToolbarSection';
import PageToolbarSeparator from 'Components/Page/Toolbar/PageToolbarSeparator';
import TableOptionsModalWrapper from 'Components/Table/TableOptions/TableOptionsModalWrapper';
import { align, icons, sortDirections } from 'Helpers/Props';
import styles from 'Movie/Index/MovieIndex.css';
import hasDifferentItemsOrOrder from 'Utilities/Object/hasDifferentItemsOrOrder';
import translate from 'Utilities/String/translate';
import getSelectedIds from 'Utilities/Table/getSelectedIds';
import selectAll from 'Utilities/Table/selectAll';
import toggleSelected from 'Utilities/Table/toggleSelected';
import DiscoverMovieFooterConnector from './DiscoverMovieFooterConnector';
import AddListMovieFilterMenu from './Menus/AddListMovieFilterMenu';
import AddListMovieSortMenu from './Menus/AddListMovieSortMenu';
import AddListMovieViewMenu from './Menus/AddListMovieViewMenu';
import NoDiscoverMovie from './NoDiscoverMovie';
import AddListMovieOverviewsConnector from './Overview/AddListMovieOverviewsConnector';
import AddListMovieOverviewOptionsModal from './Overview/Options/AddListMovieOverviewOptionsModal';
import AddListMoviePostersConnector from './Posters/AddListMoviePostersConnector';
import AddListMoviePosterOptionsModal from './Posters/Options/AddListMoviePosterOptionsModal';
import AddListMovieTableConnector from './Table/AddListMovieTableConnector';

function getViewComponent(view) {
  if (view === 'posters') {
    return AddListMoviePostersConnector;
  }

  if (view === 'overview') {
    return AddListMovieOverviewsConnector;
  }

  return AddListMovieTableConnector;
}

class DiscoverMovie extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      scroller: null,
      jumpBarItems: { order: [] },
      jumpToCharacter: null,
      isPosterOptionsModalOpen: false,
      isOverviewOptionsModalOpen: false,
      isConfirmSearchModalOpen: false,
      searchType: null,
      allSelected: false,
      allUnselected: false,
      lastToggled: null,
      selectedState: {}
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
      sortDirection
    } = this.props;

    if (sortKey !== prevProps.sortKey ||
        sortDirection !== prevProps.sortDirection ||
        hasDifferentItemsOrOrder(prevProps.items, items)
    ) {
      this.setJumpBarItems();
      this.setSelectedState();
    }

    if (this.state.jumpToCharacter != null) {
      this.setState({ jumpToCharacter: null });
    }
  }

  //
  // Control

  setScrollerRef = (ref) => {
    this.setState({ scroller: ref });
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
      const isItemSelected = selectedState[movie.tmdbId];

      if (isItemSelected) {
        newSelectedState[movie.tmdbId] = isItemSelected;
      } else {
        newSelectedState[movie.tmdbId] = false;
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
      this.setState({ jumpBarItems: { order: [] } });
      return;
    }

    const characters = _.reduce(items, (acc, item) => {
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
    if (sortDirection === sortDirections.DESCENDING) {
      order.reverse();
    }

    const jumpBarItems = {
      characters,
      order
    };

    this.setState({ jumpBarItems });
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
      return toggleSelected(state, this.props.items, id, value, shiftKey, 'tmdbId');
    });
  }

  onAddMoviesPress = ({ addOptions }) => {
    this.props.onAddMoviesPress({ ids: this.getSelectedIds(), addOptions });
  }

  onExcludeMoviesPress = () => {
    this.props.onExcludeMoviesPress({ ids: this.getSelectedIds() });
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
      onSortSelect,
      onFilterSelect,
      onViewSelect,
      onScroll,
      onAddMoviesPress,
      ...otherProps
    } = this.props;

    const {
      scroller,
      jumpBarItems,
      jumpToCharacter,
      isPosterOptionsModalOpen,
      isOverviewOptionsModalOpen,
      selectedState,
      allSelected,
      allUnselected
    } = this.state;

    const selectedMovieIds = this.getSelectedIds();

    const ViewComponent = getViewComponent(view);
    const isLoaded = !!(!error && isPopulated && items.length && scroller);
    const hasNoMovie = !totalItems;

    return (
      <PageContent>
        <PageToolbar>
          <PageToolbarSection>
            <PageToolbarButton
              label={allSelected ? 'Unselect All' : 'Select All'}
              iconName={icons.CHECK_SQUARE}
              isDisabled={hasNoMovie}
              onPress={this.onSelectAllPress}
            />
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
                >
                  <PageToolbarButton
                    label={translate('Options')}
                    iconName={icons.TABLE}
                  />
                </TableOptionsModalWrapper> :
                null
            }

            {
              view === 'posters' ?
                <PageToolbarButton
                  label={translate('Options')}
                  iconName={icons.POSTER}
                  isDisabled={hasNoMovie}
                  onPress={this.onPosterOptionsPress}
                /> :
                null
            }

            {
              view === 'overview' ?
                <PageToolbarButton
                  label={translate('Options')}
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

            <AddListMovieViewMenu
              view={view}
              isDisabled={hasNoMovie}
              onViewSelect={onViewSelect}
            />

            <AddListMovieSortMenu
              sortKey={sortKey}
              sortDirection={sortDirection}
              isDisabled={hasNoMovie}
              onSortSelect={onSortSelect}
            />

            <AddListMovieFilterMenu
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
            registerScroller={this.setScrollerRef}
            className={styles.contentBody}
            innerClassName={styles[`${view}InnerContentBody`]}
            onScroll={onScroll}
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
                    scroller={scroller}
                    items={items}
                    filters={filters}
                    sortKey={sortKey}
                    sortDirection={sortDirection}
                    jumpToCharacter={jumpToCharacter}
                    allSelected={allSelected}
                    allUnselected={allUnselected}
                    onSelectedChange={this.onSelectedChange}
                    onSelectAllChange={this.onSelectAllChange}
                    selectedState={selectedState}
                    {...otherProps}
                  />
                </div>
            }

            {
              !error && isPopulated && !items.length &&
                <NoDiscoverMovie totalItems={totalItems} />
            }
          </PageContentBody>

          {
            isLoaded && !!jumpBarItems.order.length &&
              <PageJumpBar
                items={jumpBarItems}
                onItemPress={this.onJumpBarItemPress}
              />
          }
        </div>

        {
          isLoaded &&
            <DiscoverMovieFooterConnector
              selectedIds={selectedMovieIds}
              onAddMoviesPress={this.onAddMoviesPress}
              onExcludeMoviesPress={this.onExcludeMoviesPress}
            />
        }

        <AddListMoviePosterOptionsModal
          isOpen={isPosterOptionsModalOpen}
          onModalClose={this.onPosterOptionsModalClose}
        />

        <AddListMovieOverviewOptionsModal
          isOpen={isOverviewOptionsModalOpen}
          onModalClose={this.onOverviewOptionsModalClose}
        />
      </PageContent>
    );
  }
}

DiscoverMovie.propTypes = {
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
  isSmallScreen: PropTypes.bool.isRequired,
  onSortSelect: PropTypes.func.isRequired,
  onFilterSelect: PropTypes.func.isRequired,
  onViewSelect: PropTypes.func.isRequired,
  onScroll: PropTypes.func.isRequired,
  onAddMoviesPress: PropTypes.func.isRequired,
  onExcludeMoviesPress: PropTypes.func.isRequired
};

export default DiscoverMovie;
