import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import hasDifferentItems from 'Utilities/Object/hasDifferentItems';
import getErrorMessage from 'Utilities/Object/getErrorMessage';
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
import NoArtist from 'Artist/NoArtist';
import ArtistIndexTableConnector from './Table/ArtistIndexTableConnector';
import ArtistIndexTableOptionsConnector from './Table/ArtistIndexTableOptionsConnector';
import ArtistIndexPosterOptionsModal from './Posters/Options/ArtistIndexPosterOptionsModal';
import ArtistIndexPostersConnector from './Posters/ArtistIndexPostersConnector';
import ArtistIndexBannerOptionsModal from './Banners/Options/ArtistIndexBannerOptionsModal';
import ArtistIndexBannersConnector from './Banners/ArtistIndexBannersConnector';
import ArtistIndexOverviewOptionsModal from './Overview/Options/ArtistIndexOverviewOptionsModal';
import ArtistIndexOverviewsConnector from './Overview/ArtistIndexOverviewsConnector';
import ArtistIndexFooterConnector from './ArtistIndexFooterConnector';
import ArtistIndexFilterMenu from './Menus/ArtistIndexFilterMenu';
import ArtistIndexSortMenu from './Menus/ArtistIndexSortMenu';
import ArtistIndexViewMenu from './Menus/ArtistIndexViewMenu';
import styles from './ArtistIndex.css';

function getViewComponent(view) {
  if (view === 'posters') {
    return ArtistIndexPostersConnector;
  }

  if (view === 'banners') {
    return ArtistIndexBannersConnector;
  }

  if (view === 'overview') {
    return ArtistIndexOverviewsConnector;
  }

  return ArtistIndexTableConnector;
}

class ArtistIndex extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      contentBody: null,
      jumpBarItems: [],
      jumpToCharacter: null,
      isPosterOptionsModalOpen: false,
      isBannerOptionsModalOpen: false,
      isOverviewOptionsModalOpen: false,
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

    // Reset if not sorting by sortName
    if (sortKey !== 'sortName') {
      this.setState({ jumpBarItems: [] });
      return;
    }

    const characters = _.reduce(items, (acc, item) => {
      const firstCharacter = item.sortName.charAt(0);

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
      isRefreshingArtist,
      isRssSyncExecuting,
      scrollTop,
      onSortSelect,
      onFilterSelect,
      onViewSelect,
      onRefreshArtistPress,
      onRssSyncPress,
      ...otherProps
    } = this.props;

    const {
      contentBody,
      jumpBarItems,
      jumpToCharacter,
      isPosterOptionsModalOpen,
      isBannerOptionsModalOpen,
      isOverviewOptionsModalOpen,
      isRendered
    } = this.state;

    const ViewComponent = getViewComponent(view);
    const isLoaded = !!(!error && isPopulated && items.length && contentBody);
    const hasNoArtist = !totalItems;

    return (
      <PageContent>
        <PageToolbar>
          <PageToolbarSection>
            <PageToolbarButton
              label="Update all"
              iconName={icons.REFRESH}
              spinningName={icons.REFRESH}
              isSpinning={isRefreshingArtist}
              isDisabled={hasNoArtist}
              onPress={onRefreshArtistPress}
            />

            <PageToolbarButton
              label="RSS Sync"
              iconName={icons.RSS}
              isSpinning={isRssSyncExecuting}
              isDisabled={hasNoArtist}
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
                  optionsComponent={ArtistIndexTableOptionsConnector}
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
                  isDisabled={hasNoArtist}
                  onPress={this.onPosterOptionsPress}
                /> :
                null
            }

            {
              view === 'banners' ?
                <PageToolbarButton
                  label="Options"
                  iconName={icons.POSTER}
                  isDisabled={hasNoArtist}
                  onPress={this.onBannerOptionsPress}
                /> :
                null
            }

            {
              view === 'overview' ?
                <PageToolbarButton
                  label="Options"
                  iconName={icons.OVERVIEW}
                  isDisabled={hasNoArtist}
                  onPress={this.onOverviewOptionsPress}
                /> :
                null
            }

            {
              (view === 'posters' || view === 'banners' || view === 'overview') &&

                <PageToolbarSeparator />
            }

            <ArtistIndexViewMenu
              view={view}
              isDisabled={hasNoArtist}
              onViewSelect={onViewSelect}
            />

            <ArtistIndexSortMenu
              sortKey={sortKey}
              sortDirection={sortDirection}
              isDisabled={hasNoArtist}
              onSortSelect={onSortSelect}
            />

            <ArtistIndexFilterMenu
              selectedFilterKey={selectedFilterKey}
              filters={filters}
              customFilters={customFilters}
              isDisabled={hasNoArtist}
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
                <div className={styles.errorMessage}>
                  {getErrorMessage(error, 'Failed to load artist from API')}
                </div>
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

                  <ArtistIndexFooterConnector />
                </div>
            }

            {
              !error && isPopulated && !items.length &&
                <NoArtist totalItems={totalItems} />
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

        <ArtistIndexPosterOptionsModal
          isOpen={isPosterOptionsModalOpen}
          onModalClose={this.onPosterOptionsModalClose}
        />

        <ArtistIndexBannerOptionsModal
          isOpen={isBannerOptionsModalOpen}
          onModalClose={this.onBannerOptionsModalClose}

        />

        <ArtistIndexOverviewOptionsModal
          isOpen={isOverviewOptionsModalOpen}
          onModalClose={this.onOverviewOptionsModalClose}

        />
      </PageContent>
    );
  }
}

ArtistIndex.propTypes = {
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
  isRefreshingArtist: PropTypes.bool.isRequired,
  isRssSyncExecuting: PropTypes.bool.isRequired,
  scrollTop: PropTypes.number.isRequired,
  isSmallScreen: PropTypes.bool.isRequired,
  onSortSelect: PropTypes.func.isRequired,
  onFilterSelect: PropTypes.func.isRequired,
  onViewSelect: PropTypes.func.isRequired,
  onRefreshArtistPress: PropTypes.func.isRequired,
  onRssSyncPress: PropTypes.func.isRequired,
  onScroll: PropTypes.func.isRequired
};

export default ArtistIndex;
