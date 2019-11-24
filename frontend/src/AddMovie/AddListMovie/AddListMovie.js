import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import hasDifferentItems from 'Utilities/Object/hasDifferentItems';
import { align, icons, sortDirections } from 'Helpers/Props';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import PageContent from 'Components/Page/PageContent';
import PageContentBodyConnector from 'Components/Page/PageContentBodyConnector';
import PageJumpBar from 'Components/Page/PageJumpBar';
import TableOptionsModalWrapper from 'Components/Table/TableOptions/TableOptionsModalWrapper';
import PageToolbar from 'Components/Page/Toolbar/PageToolbar';
import PageToolbarSeparator from 'Components/Page/Toolbar/PageToolbarSeparator';
import PageToolbarSection from 'Components/Page/Toolbar/PageToolbarSection';
import PageToolbarButton from 'Components/Page/Toolbar/PageToolbarButton';
import AddListMovieTableConnector from './Table/AddListMovieTableConnector';
import AddListMoviePosterOptionsModal from './Posters/Options/AddListMoviePosterOptionsModal';
import AddListMoviePostersConnector from './Posters/AddListMoviePostersConnector';
import AddListMovieOverviewOptionsModal from './Overview/Options/AddListMovieOverviewOptionsModal';
import AddListMovieOverviewsConnector from './Overview/AddListMovieOverviewsConnector';
import AddListMovieFilterMenu from 'AddMovie/AddListMovie/Menus/AddListMovieFilterMenu';
import AddListMovieSortMenu from 'AddMovie/AddListMovie/Menus/AddListMovieSortMenu';
import AddListMovieViewMenu from 'AddMovie/AddListMovie/Menus/AddListMovieViewMenu';
import styles from 'Movie/Index/MovieIndex.css';

function getViewComponent(view) {
  if (view === 'posters') {
    return AddListMoviePostersConnector;
  }

  if (view === 'overview') {
    return AddListMovieOverviewsConnector;
  }

  return AddListMovieTableConnector;
}

class AddListMovie extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      contentBody: null,
      jumpBarItems: { order: [] },
      jumpToCharacter: null,
      isPosterOptionsModalOpen: false,
      isOverviewOptionsModalOpen: false,
      isConfirmSearchModalOpen: false,
      searchType: null,
      lastToggled: null,
      isRendered: false
    };
  }

  componentDidMount() {
    this.setJumpBarItems();
  }

  componentDidUpdate(prevProps) {
    const {
      items,
      sortKey,
      sortDirection,
      scrollTop
    } = this.props;

    if (
      hasDifferentItems(prevProps.items, items) ||
      sortKey !== prevProps.sortKey ||
      sortDirection !== prevProps.sortDirection
    ) {
      this.setJumpBarItems();
    }

    if (this.state.jumpToCharacter != null && scrollTop !== prevProps.scrollTop) {
      this.setState({ jumpToCharacter: null });
    }
  }

  //
  // Control

  setContentBodyRef = (ref) => {
    this.setState({ contentBody: ref });
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
      scrollTop,
      onSortSelect,
      onFilterSelect,
      onViewSelect,
      ...otherProps
    } = this.props;

    const {
      contentBody,
      jumpBarItems,
      jumpToCharacter,
      isPosterOptionsModalOpen,
      isOverviewOptionsModalOpen,
      isRendered
    } = this.state;

    const ViewComponent = getViewComponent(view);
    const isLoaded = !!(!error && isPopulated && items.length && contentBody);
    const hasNoMovie = !totalItems;

    return (
      <PageContent>
        <PageToolbar>
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
                    {...otherProps}
                  />
                </div>
            }

            {
              !error && isPopulated && !items.length &&
                <div className={styles.message}>
                  <div className={styles.noResults}>Couldn't find any results</div>
                </div>
            }
          </PageContentBodyConnector>

          {
            isLoaded && !!jumpBarItems.order.length &&
              <PageJumpBar
                items={jumpBarItems}
                onItemPress={this.onJumpBarItemPress}
              />
          }
        </div>

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

AddListMovie.propTypes = {
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
  scrollTop: PropTypes.number.isRequired,
  isSmallScreen: PropTypes.bool.isRequired,
  onSortSelect: PropTypes.func.isRequired,
  onFilterSelect: PropTypes.func.isRequired,
  onViewSelect: PropTypes.func.isRequired,
  onScroll: PropTypes.func.isRequired
};

export default AddListMovie;
