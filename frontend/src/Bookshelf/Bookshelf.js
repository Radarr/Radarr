import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { CellMeasurer, CellMeasurerCache } from 'react-virtualized';
import NoAuthor from 'Author/NoAuthor';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import FilterMenu from 'Components/Menu/FilterMenu';
import PageContent from 'Components/Page/PageContent';
import PageContentBody from 'Components/Page/PageContentBody';
import PageJumpBar from 'Components/Page/PageJumpBar';
import PageToolbar from 'Components/Page/Toolbar/PageToolbar';
import PageToolbarSection from 'Components/Page/Toolbar/PageToolbarSection';
import VirtualTable from 'Components/Table/VirtualTable';
import VirtualTableRow from 'Components/Table/VirtualTableRow';
import { align, sortDirections } from 'Helpers/Props';
import getIndexOfFirstCharacter from 'Utilities/Array/getIndexOfFirstCharacter';
import getErrorMessage from 'Utilities/Object/getErrorMessage';
import getSelectedIds from 'Utilities/Table/getSelectedIds';
import selectAll from 'Utilities/Table/selectAll';
import toggleSelected from 'Utilities/Table/toggleSelected';
import BookStudioFilterModalConnector from './BookshelfFilterModalConnector';
import BookshelfFooter from './BookshelfFooter';
import BookStudioRowConnector from './BookshelfRowConnector';
import BookshelfTableHeader from './BookshelfTableHeader';
import styles from './Bookshelf.css';

const columns = [
  {
    name: 'monitored',
    isVisible: true
  },
  {
    name: 'status',
    isVisible: true
  },
  {
    name: 'sortName',
    label: 'Name',
    isSortable: true,
    isVisible: true
  },
  {
    name: 'bookCount',
    label: 'Books',
    isSortable: false,
    isVisible: true
  }
];

class Bookshelf extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      estimatedRowSize: 100,
      scroller: null,
      jumpBarItems: { order: [] },
      scrollIndex: null,
      jumpCount: 0,
      allSelected: false,
      allUnselected: false,
      lastToggled: null,
      selectedState: {}
    };

    this.cache = new CellMeasurerCache({
      defaultHeight: 100,
      fixedWidth: true
    });
  }

  componentDidMount() {
    this.setSelectedState();
  }

  componentDidUpdate(prevProps) {
    const {
      isSaving,
      saveError
    } = this.props;

    const {
      scrollIndex,
      jumpCount
    } = this.state;

    if (prevProps.isSaving && !isSaving && !saveError) {
      this.onSelectAllChange({ value: false });
    }

    // nasty hack to fix react-virtualized jumping incorrectly
    // due to variable row heights
    if (scrollIndex != null) {
      if (jumpCount === 0) {
        this.setState({
          scrollIndex: scrollIndex + 1,
          jumpCount: 1
        });
      } else if (jumpCount === 1) {
        this.setState({
          scrollIndex: scrollIndex - 1,
          jumpCount: 2
        });
      } else {
        this.setState({
          scrollIndex: null,
          jumpCount: 0
        });
      }
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

  getSelectedIds = () => {
    if (this.state.allUnselected) {
      return [];
    }
    return getSelectedIds(this.state.selectedState);
  }

  setSelectedState = () => {
    const {
      items
    } = this.props;

    const {
      selectedState
    } = this.state;

    const newSelectedState = {};

    items.forEach((author) => {
      const isItemSelected = selectedState[author.id];

      if (isItemSelected) {
        newSelectedState[author.id] = isItemSelected;
      } else {
        newSelectedState[author.id] = false;
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

  estimateRowHeight = (width) => {
    const {
      bookCount,
      items
    } = this.props;

    if (bookCount === undefined || bookCount === 0 || items.length === 0) {
      return 100;
    }

    // guess 250px per book entry
    // available width is total width less 186px for select, status etc
    const cols = Math.max(Math.floor((width - 186) / 250), 1);
    const booksPerAuthor = bookCount / items.length;
    const bookRowsPerAuthor = booksPerAuthor / cols;

    // each row is 23px per book row plus 16px padding
    return bookRowsPerAuthor * 23 + 16;
  }

  rowRenderer = ({ key, rowIndex, parent, style }) => {
    const {
      items
    } = this.props;

    const {
      selectedState
    } = this.state;

    const item = items[rowIndex];

    return (
      <CellMeasurer
        key={key}
        cache={this.cache}
        parent={parent}
        columnIndex={0}
        rowIndex={rowIndex}
      >
        {({ registerChild }) => (
          <VirtualTableRow
            ref={registerChild}
            style={style}
          >
            <BookStudioRowConnector
              key={item.id}
              authorId={item.id}
              isSelected={selectedState[item.id]}
              onSelectedChange={this.onSelectedChange}
            />
          </VirtualTableRow>
        )}
      </CellMeasurer>
    );
  }

  //
  // Listeners

  onSelectAllChange = ({ value }) => {
    this.setState(selectAll(this.state.selectedState, value));
  }

  onSelectedChange = ({ id, value, shiftKey = false }) => {
    this.setState((state) => {
      return toggleSelected(state, this.props.items, id, value, shiftKey);
    });
  }

  onSelectAllPress = () => {
    this.onSelectAllChange({ value: !this.state.allSelected });
  }

  onUpdateSelectedPress = (changes) => {
    this.props.onUpdateSelectedPress({
      authorIds: this.getSelectedIds(),
      ...changes
    });
  }

  onJumpBarItemPress = (jumpToCharacter) => {
    const scrollIndex = getIndexOfFirstCharacter(this.props.items, jumpToCharacter);

    if (scrollIndex != null) {
      this.setState({ scrollIndex });
    }
  }

  onGridRecompute = (width) => {
    this.setJumpBarItems();
    this.setSelectedState();
    this.setState({ estimatedRowSize: this.estimateRowHeight(width) });
    this.cache.clearAll();
  }

  // onSectionRendered = ({ rowStartIndex }) => {
  //   console.log(`rendered starting ${rowStartIndex}, aiming for ${this.state.scrollIndex}`);

  //   const {
  //     scrollIndex
  //   } = this.state;

  //   if (rowStartIndex === scrollIndex) {
  //     console.log('resetting scrollindex');
  //     this.setState({ scrollIndex: null });
  //   }
  // }

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
      isSaving,
      saveError,
      isSmallScreen,
      onSortPress,
      onFilterSelect
    } = this.props;

    const {
      allSelected,
      allUnselected,
      estimatedRowSize,
      scroller,
      jumpBarItems,
      scrollIndex
    } = this.state;

    return (
      <PageContent title="Book Studio">
        <PageToolbar>
          <PageToolbarSection />
          <PageToolbarSection alignContent={align.RIGHT}>
            <FilterMenu
              alignMenu={align.RIGHT}
              selectedFilterKey={selectedFilterKey}
              filters={filters}
              customFilters={customFilters}
              filterModalConnectorComponent={BookStudioFilterModalConnector}
              onFilterSelect={onFilterSelect}
            />
          </PageToolbarSection>
        </PageToolbar>

        <div className={styles.pageContentBodyWrapper}>
          <PageContentBody
            registerScroller={this.setScrollerRef}
            className={styles.contentBody}
            innerClassName={styles.innerContentBody}
          >
            {
              isFetching && !isPopulated &&
                <LoadingIndicator />
            }

            {
              !isFetching && !!error &&
                <div>{getErrorMessage(error, 'Failed to load author from API')}</div>
            }

            {
              !error && isPopulated && !!items.length &&
                <div className={styles.contentBodyContainer}>
                  <VirtualTable
                    items={items}
                    scrollIndex={scrollIndex}
                    columns={columns}
                    scroller={scroller}
                    isSmallScreen={isSmallScreen}
                    overscanRowCount={5}
                    rowRenderer={this.rowRenderer}
                    header={
                      <BookshelfTableHeader
                        columns={columns}
                        sortKey={sortKey}
                        sortDirection={sortDirection}
                        onSortPress={onSortPress}
                        allSelected={allSelected}
                        allUnselected={allUnselected}
                        onSelectAllChange={this.onSelectAllChange}
                      />
                    }
                    sortKey={sortKey}
                    sortDirection={sortDirection}
                    deferredMeasurementCache={this.cache}
                    rowHeight={this.cache.rowHeight}
                    estimatedRowSize={estimatedRowSize}
                    onRecompute={this.onGridRecompute}
                  />
                </div>
            }

            {
              !error && isPopulated && !items.length &&
                <NoAuthor totalItems={totalItems} />
            }
          </PageContentBody>

          {
            isPopulated && !!jumpBarItems.order.length &&
              <PageJumpBar
                items={jumpBarItems}
                onItemPress={this.onJumpBarItemPress}
              />
          }
        </div>

        <BookshelfFooter
          selectedCount={this.getSelectedIds().length}
          isSaving={isSaving}
          saveError={saveError}
          onUpdateSelectedPress={this.onUpdateSelectedPress}
        />
      </PageContent>
    );
  }
}

Bookshelf.propTypes = {
  isFetching: PropTypes.bool.isRequired,
  isPopulated: PropTypes.bool.isRequired,
  error: PropTypes.object,
  totalItems: PropTypes.number.isRequired,
  items: PropTypes.arrayOf(PropTypes.object).isRequired,
  bookCount: PropTypes.number.isRequired,
  sortKey: PropTypes.string,
  sortDirection: PropTypes.oneOf(sortDirections.all),
  selectedFilterKey: PropTypes.oneOfType([PropTypes.string, PropTypes.number]).isRequired,
  filters: PropTypes.arrayOf(PropTypes.object).isRequired,
  customFilters: PropTypes.arrayOf(PropTypes.object).isRequired,
  isSaving: PropTypes.bool.isRequired,
  saveError: PropTypes.object,
  isSmallScreen: PropTypes.bool.isRequired,
  onSortPress: PropTypes.func.isRequired,
  onFilterSelect: PropTypes.func.isRequired,
  onUpdateSelectedPress: PropTypes.func.isRequired
};

export default Bookshelf;
