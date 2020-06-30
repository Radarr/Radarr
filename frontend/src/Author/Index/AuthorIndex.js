import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import NoAuthor from 'Author/NoAuthor';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import PageContent from 'Components/Page/PageContent';
import PageContentBodyConnector from 'Components/Page/PageContentBodyConnector';
import PageJumpBar from 'Components/Page/PageJumpBar';
import PageToolbar from 'Components/Page/Toolbar/PageToolbar';
import PageToolbarButton from 'Components/Page/Toolbar/PageToolbarButton';
import PageToolbarSection from 'Components/Page/Toolbar/PageToolbarSection';
import PageToolbarSeparator from 'Components/Page/Toolbar/PageToolbarSeparator';
import TableOptionsModalWrapper from 'Components/Table/TableOptions/TableOptionsModalWrapper';
import { align, icons, sortDirections } from 'Helpers/Props';
import getErrorMessage from 'Utilities/Object/getErrorMessage';
import hasDifferentItemsOrOrder from 'Utilities/Object/hasDifferentItemsOrOrder';
import AuthorIndexFooterConnector from './AuthorIndexFooterConnector';
import AuthorIndexBannersConnector from './Banners/AuthorIndexBannersConnector';
import AuthorIndexBannerOptionsModal from './Banners/Options/AuthorIndexBannerOptionsModal';
import AuthorIndexFilterMenu from './Menus/AuthorIndexFilterMenu';
import AuthorIndexSortMenu from './Menus/AuthorIndexSortMenu';
import AuthorIndexViewMenu from './Menus/AuthorIndexViewMenu';
import AuthorIndexOverviewsConnector from './Overview/AuthorIndexOverviewsConnector';
import AuthorIndexOverviewOptionsModal from './Overview/Options/AuthorIndexOverviewOptionsModal';
import AuthorIndexPostersConnector from './Posters/AuthorIndexPostersConnector';
import AuthorIndexPosterOptionsModal from './Posters/Options/AuthorIndexPosterOptionsModal';
import AuthorIndexTableConnector from './Table/AuthorIndexTableConnector';
import AuthorIndexTableOptionsConnector from './Table/AuthorIndexTableOptionsConnector';
import styles from './AuthorIndex.css';

function getViewComponent(view) {
  if (view === 'posters') {
    return AuthorIndexPostersConnector;
  }

  if (view === 'banners') {
    return AuthorIndexBannersConnector;
  }

  if (view === 'overview') {
    return AuthorIndexOverviewsConnector;
  }

  return AuthorIndexTableConnector;
}

class AuthorIndex extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      scroller: null,
      jumpBarItems: { order: [] },
      jumpToCharacter: null,
      isPosterOptionsModalOpen: false,
      isBannerOptionsModalOpen: false,
      isOverviewOptionsModalOpen: false
    };
  }

  componentDidMount() {
    this.setJumpBarItems();
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

  setJumpBarItems() {
    const {
      items,
      sortKey,
      sortDirection
    } = this.props;

    // Reset if not sorting by sortName
    if (sortKey !== 'sortName') {
      this.setState({ jumpBarItems: { order: [] } });
      return;
    }

    const characters = _.reduce(items, (acc, item) => {
      let char = item.sortName.charAt(0);

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

  onBannerOptionsPress = () => {
    this.setState({ isBannerOptionsModalOpen: true });
  }

  onBannerOptionsModalClose = () => {
    this.setState({ isBannerOptionsModalOpen: false });
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
      isRefreshingAuthor,
      isRssSyncExecuting,
      onScroll,
      onSortSelect,
      onFilterSelect,
      onViewSelect,
      onRefreshAuthorPress,
      onRssSyncPress,
      ...otherProps
    } = this.props;

    const {
      scroller,
      jumpBarItems,
      jumpToCharacter,
      isPosterOptionsModalOpen,
      isBannerOptionsModalOpen,
      isOverviewOptionsModalOpen
    } = this.state;

    const ViewComponent = getViewComponent(view);
    const isLoaded = !!(!error && isPopulated && items.length && scroller);
    const hasNoAuthor = !totalItems;

    return (
      <PageContent>
        <PageToolbar>
          <PageToolbarSection>
            <PageToolbarButton
              label="Update all"
              iconName={icons.REFRESH}
              spinningName={icons.REFRESH}
              isSpinning={isRefreshingAuthor}
              onPress={onRefreshAuthorPress}
            />

            <PageToolbarButton
              label="RSS Sync"
              iconName={icons.RSS}
              isSpinning={isRssSyncExecuting}
              isDisabled={hasNoAuthor}
              onPress={onRssSyncPress}
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
                  optionsComponent={AuthorIndexTableOptionsConnector}
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
                  isDisabled={hasNoAuthor}
                  onPress={this.onPosterOptionsPress}
                /> :
                null
            }

            {
              view === 'banners' ?
                <PageToolbarButton
                  label="Options"
                  iconName={icons.POSTER}
                  isDisabled={hasNoAuthor}
                  onPress={this.onBannerOptionsPress}
                /> :
                null
            }

            {
              view === 'overview' ?
                <PageToolbarButton
                  label="Options"
                  iconName={icons.OVERVIEW}
                  isDisabled={hasNoAuthor}
                  onPress={this.onOverviewOptionsPress}
                /> :
                null
            }

            <PageToolbarSeparator />

            <AuthorIndexViewMenu
              view={view}
              isDisabled={hasNoAuthor}
              onViewSelect={onViewSelect}
            />

            <AuthorIndexSortMenu
              sortKey={sortKey}
              sortDirection={sortDirection}
              isDisabled={hasNoAuthor}
              onSortSelect={onSortSelect}
            />

            <AuthorIndexFilterMenu
              selectedFilterKey={selectedFilterKey}
              filters={filters}
              customFilters={customFilters}
              isDisabled={hasNoAuthor}
              onFilterSelect={onFilterSelect}
            />
          </PageToolbarSection>
        </PageToolbar>

        <div className={styles.pageContentBodyWrapper}>
          <PageContentBodyConnector
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
                <div className={styles.errorMessage}>
                  {getErrorMessage(error, 'Failed to load author from API')}
                </div>
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
                    {...otherProps}
                  />

                  <AuthorIndexFooterConnector />
                </div>
            }

            {
              !error && isPopulated && !items.length &&
                <NoAuthor totalItems={totalItems} />
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

        <AuthorIndexPosterOptionsModal
          isOpen={isPosterOptionsModalOpen}
          onModalClose={this.onPosterOptionsModalClose}
        />

        <AuthorIndexBannerOptionsModal
          isOpen={isBannerOptionsModalOpen}
          onModalClose={this.onBannerOptionsModalClose}

        />

        <AuthorIndexOverviewOptionsModal
          isOpen={isOverviewOptionsModalOpen}
          onModalClose={this.onOverviewOptionsModalClose}

        />
      </PageContent>
    );
  }
}

AuthorIndex.propTypes = {
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
  isRefreshingAuthor: PropTypes.bool.isRequired,
  isRssSyncExecuting: PropTypes.bool.isRequired,
  isSmallScreen: PropTypes.bool.isRequired,
  onSortSelect: PropTypes.func.isRequired,
  onFilterSelect: PropTypes.func.isRequired,
  onViewSelect: PropTypes.func.isRequired,
  onRefreshAuthorPress: PropTypes.func.isRequired,
  onRssSyncPress: PropTypes.func.isRequired,
  onScroll: PropTypes.func.isRequired
};

export default AuthorIndex;
