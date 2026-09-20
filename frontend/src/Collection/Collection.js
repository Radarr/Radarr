import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import Alert from 'Components/Alert';
import TextInput from 'Components/Form/TextInput';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import PageContent from 'Components/Page/PageContent';
import PageContentBody from 'Components/Page/PageContentBody';
import PageJumpBar from 'Components/Page/PageJumpBar';
import PageToolbar from 'Components/Page/Toolbar/PageToolbar';
import PageToolbarButton from 'Components/Page/Toolbar/PageToolbarButton';
import PageToolbarSection from 'Components/Page/Toolbar/PageToolbarSection';
import PageToolbarSeparator from 'Components/Page/Toolbar/PageToolbarSeparator';
import { align, icons, kinds, sortDirections } from 'Helpers/Props';
import styles from 'Movie/Index/MovieIndex.css';
import hasDifferentItemsOrOrder from 'Utilities/Object/hasDifferentItemsOrOrder';
import translate from 'Utilities/String/translate';
import getSelectedIds from 'Utilities/Table/getSelectedIds';
import selectAll from 'Utilities/Table/selectAll';
import toggleSelected from 'Utilities/Table/toggleSelected';
import CollectionFooter from './CollectionFooter';
import MovieCollectionFilterMenu from './Menus/MovieCollectionFilterMenu';
import MovieCollectionSortMenu from './Menus/MovieCollectionSortMenu';
import NoMovieCollections from './NoMovieCollections';
import CollectionOverviewsConnector from './Overview/CollectionOverviewsConnector';
import CollectionOverviewOptionsModal from './Overview/Options/CollectionOverviewOptionsModal';
import collectionStyles from './Collection.css';

function getViewComponent(view) {
  return CollectionOverviewsConnector;
}

class Collection extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.scrollerRef = React.createRef();

    this.setFilterText = _.debounce((filterText) => {
      this.setState({ filterText });
    }, 250);

    this.state = {
      searchInput: '',
      filterText: '',
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

  componentDidUpdate(prevProps, prevState) {
    const {
      items,
      sortKey,
      sortDirection
    } = this.props;

    if (sortKey !== prevProps.sortKey ||
        sortDirection !== prevProps.sortDirection ||
        this.state.filterText !== prevState.filterText ||
        hasDifferentItemsOrOrder(prevProps.items, items)
    ) {
      this.setJumpBarItems();
      this.setSelectedState();
    }

    if (this.state.jumpToCharacter != null) {
      this.setState({ jumpToCharacter: null });
    }
  }

  componentWillUnmount() {
    this.setFilterText.cancel();
  }

  //
  // Control

  getSelectedIds = () => {
    if (this.state.allUnselected) {
      return [];
    }
    return getSelectedIds(this.state.selectedState);
  };

  getFilteredItems() {
    const { items } = this.props;
    const filterText = this.state.filterText.trim().toLowerCase();

    if (!filterText) {
      return items;
    }

    return items.filter((item) => {
      return item.sortTitle && item.sortTitle.toLowerCase().includes(filterText);
    });
  }

  setSelectedState() {
    const items = this.getFilteredItems();

    const {
      selectedState
    } = this.state;

    const newSelectedState = {};

    items.forEach((collection) => {
      const isItemSelected = selectedState[collection.id];

      if (isItemSelected) {
        newSelectedState[collection.id] = isItemSelected;
      } else {
        newSelectedState[collection.id] = false;
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
      sortKey,
      sortDirection
    } = this.props;

    const items = this.getFilteredItems();

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

  onOverviewOptionsPress = () => {
    this.setState({ isOverviewOptionsModalOpen: true });
  };

  onOverviewOptionsModalClose = () => {
    this.setState({ isOverviewOptionsModalOpen: false });
  };

  onJumpBarItemPress = (jumpToCharacter) => {
    this.setState({ jumpToCharacter });
  };

  onSearchInputChange = ({ value }) => {
    this.setState({ searchInput: value });
    this.setFilterText(value);
  };

  onSelectAllChange = ({ value }) => {
    this.setState(selectAll(this.state.selectedState, value));
  };

  onSelectAllPress = () => {
    this.onSelectAllChange({ value: !this.state.allSelected });
  };

  onRefreshMovieCollectionsPress = () => {
    this.props.onRefreshMovieCollectionsPress();
  };

  onSelectedChange = ({ id, value, shiftKey = false }) => {
    this.setState((state) => {
      return toggleSelected(state, this.props.items, id, value, shiftKey, 'id');
    });
  };

  onUpdateSelectedPress = (changes) => {
    this.props.onUpdateSelectedPress({
      collectionIds: this.getSelectedIds(),
      ...changes
    });
  };

  //
  // Render

  render() {
    const {
      isFetching,
      isPopulated,
      error,
      totalItems,
      items,
      selectedFilterKey,
      filters,
      customFilters,
      sortKey,
      sortDirection,
      view,
      onSortSelect,
      onFilterSelect,
      initialScrollTop,
      onScroll,
      isRefreshingCollections,
      isSaving,
      isAdding,
      ...otherProps
    } = this.props;

    const {
      searchInput,
      jumpBarItems,
      jumpToCharacter,
      isOverviewOptionsModalOpen,
      selectedState,
      allSelected,
      allUnselected
    } = this.state;

    const selectedMovieIds = this.getSelectedIds();

    const filteredItems = this.getFilteredItems();
    const ViewComponent = getViewComponent(view);
    const isLoaded = !!(!error && isPopulated && filteredItems.length && this.scrollerRef.current);
    const hasNoCollection = !totalItems;

    return (
      <PageContent title={translate('Collections')}>
        <PageToolbar>
          <PageToolbarSection>
            <PageToolbarButton
              label={translate('RefreshCollections')}
              iconName={icons.REFRESH}
              isSpinning={isRefreshingCollections}
              isDisabled={hasNoCollection}
              onPress={this.onRefreshMovieCollectionsPress}
            />
            <PageToolbarButton
              label={allSelected ? translate('UnselectAll') : translate('SelectAll')}
              iconName={icons.CHECK_SQUARE}
              isDisabled={hasNoCollection}
              onPress={this.onSelectAllPress}
            />
          </PageToolbarSection>

          <PageToolbarSection
            alignContent={align.RIGHT}
            collapseButtons={false}
          >
            {
              !hasNoCollection &&
                <div className={collectionStyles.searchInput}>
                  <TextInput
                    name="collectionSearch"
                    value={searchInput}
                    placeholder={translate('Search')}
                    onChange={this.onSearchInputChange}
                  />
                </div>
            }

            {
              view === 'overview' ?
                <PageToolbarButton
                  label={translate('Options')}
                  iconName={icons.OVERVIEW}
                  onPress={this.onOverviewOptionsPress}
                /> :
                null
            }

            {
              (view === 'posters' || view === 'overview') &&
                <PageToolbarSeparator />
            }

            <MovieCollectionSortMenu
              sortKey={sortKey}
              sortDirection={sortDirection}
              isDisabled={hasNoCollection}
              onSortSelect={onSortSelect}
            />

            <MovieCollectionFilterMenu
              selectedFilterKey={selectedFilterKey}
              filters={filters}
              customFilters={customFilters}
              isDisabled={hasNoCollection}
              onFilterSelect={onFilterSelect}
            />
          </PageToolbarSection>
        </PageToolbar>

        <div className={styles.pageContentBodyWrapper}>
          <PageContentBody
            ref={this.scrollerRef}
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
                <Alert kind={kinds.DANGER}>
                  {translate('UnableToLoadCollections')}
                </Alert>
            }

            {
              isLoaded &&
                <div className={styles.contentBodyContainer}>
                  <ViewComponent
                    scroller={this.scrollerRef.current}
                    items={filteredItems}
                    filters={filters}
                    sortKey={sortKey}
                    sortDirection={sortDirection}
                    jumpToCharacter={jumpToCharacter}
                    allSelected={allSelected}
                    allUnselected={allUnselected}
                    onSelectedChange={this.onSelectedChange}
                    onSelectAllChange={this.onSelectAllChange}
                    selectedState={selectedState}
                    scrollTop={initialScrollTop}
                    {...otherProps}
                  />
                </div>
            }

            {
              !error && isPopulated && !filteredItems.length &&
                <NoMovieCollections totalItems={totalItems} />
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
            <CollectionFooter
              selectedIds={selectedMovieIds}
              isSaving={isSaving}
              isAdding={isAdding}
              onUpdateSelectedPress={this.onUpdateSelectedPress}
            />
        }

        <CollectionOverviewOptionsModal
          isOpen={isOverviewOptionsModalOpen}
          onModalClose={this.onOverviewOptionsModalClose}
        />
      </PageContent>
    );
  }
}

Collection.propTypes = {
  initialScrollTop: PropTypes.number,
  isFetching: PropTypes.bool.isRequired,
  isPopulated: PropTypes.bool.isRequired,
  isSaving: PropTypes.bool.isRequired,
  isAdding: PropTypes.bool.isRequired,
  error: PropTypes.object,
  totalItems: PropTypes.number.isRequired,
  items: PropTypes.arrayOf(PropTypes.object).isRequired,
  selectedFilterKey: PropTypes.oneOfType([PropTypes.string, PropTypes.number]).isRequired,
  filters: PropTypes.arrayOf(PropTypes.object).isRequired,
  customFilters: PropTypes.arrayOf(PropTypes.object).isRequired,
  sortKey: PropTypes.string,
  sortDirection: PropTypes.oneOf(sortDirections.all),
  view: PropTypes.string.isRequired,
  isRefreshingCollections: PropTypes.bool.isRequired,
  isSmallScreen: PropTypes.bool.isRequired,
  onSortSelect: PropTypes.func.isRequired,
  onFilterSelect: PropTypes.func.isRequired,
  onScroll: PropTypes.func.isRequired,
  onUpdateSelectedPress: PropTypes.func.isRequired,
  onRefreshMovieCollectionsPress: PropTypes.func.isRequired
};

export default Collection;
