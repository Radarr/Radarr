import PropTypes from 'prop-types';
import React, { Component } from 'react';
import getIndexOfFirstCharacter from 'Utilities/Array/getIndexOfFirstCharacter';
import { sortDirections } from 'Helpers/Props';
import VirtualTable from 'Components/Table/VirtualTable';
import VirtualTableRow from 'Components/Table/VirtualTableRow';
import MovieIndexItemConnector from 'Movie/Index/MovieIndexItemConnector';
import MovieIndexHeaderConnector from './MovieIndexHeaderConnector';
import MovieIndexRow from './MovieIndexRow';
import styles from './MovieIndexTable.css';

class MovieIndexTable extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      scrollIndex: null
    };
  }

  componentDidUpdate(prevProps) {
    const {
      items
    } = this.props;

    const jumpToCharacter = this.props.jumpToCharacter;

    if (jumpToCharacter != null && jumpToCharacter !== prevProps.jumpToCharacter) {

      const scrollIndex = getIndexOfFirstCharacter(items, jumpToCharacter);

      if (scrollIndex != null) {
        this.setState({ scrollIndex });
      }
    } else if (jumpToCharacter == null && prevProps.jumpToCharacter != null) {
      this.setState({ scrollIndex: null });
    }
  }

  //
  // Control

  rowRenderer = ({ key, rowIndex, style }) => {
    const {
      items,
      columns,
      selectedState,
      onSelectedChange,
      isMovieEditorActive
    } = this.props;

    const movie = items[rowIndex];

    return (
      <VirtualTableRow
        key={key}
        style={style}
      >
        <MovieIndexItemConnector
          key={rowIndex}
          component={MovieIndexRow}
          columns={columns}
          movieId={movie.id}
          qualityProfileId={movie.qualityProfileId}
          isSelected={selectedState[movie.id]}
          onSelectedChange={onSelectedChange}
          isMovieEditorActive={isMovieEditorActive}
        />
      </VirtualTableRow>
    );
  }

  //
  // Render

  render() {
    const {
      items,
      columns,
      filters,
      sortKey,
      sortDirection,
      isSmallScreen,
      scrollTop,
      contentBody,
      onSortPress,
      onRender,
      onScroll,
      allSelected,
      allUnselected,
      onSelectAllChange,
      isMovieEditorActive,
      selectedState
    } = this.props;

    return (
      <VirtualTable
        className={styles.tableContainer}
        items={items}
        scrollTop={scrollTop}
        scrollIndex={this.state.scrollIndex}
        contentBody={contentBody}
        isSmallScreen={isSmallScreen}
        rowHeight={38}
        overscanRowCount={2}
        rowRenderer={this.rowRenderer}
        header={
          <MovieIndexHeaderConnector
            columns={columns}
            sortKey={sortKey}
            sortDirection={sortDirection}
            onSortPress={onSortPress}
            allSelected={allSelected}
            allUnselected={allUnselected}
            onSelectAllChange={onSelectAllChange}
            isMovieEditorActive={isMovieEditorActive}
          />
        }
        selectedState={selectedState}
        columns={columns}
        filters={filters}
        sortKey={sortKey}
        sortDirection={sortDirection}
        onRender={onRender}
        onScroll={onScroll}
        isScrollingOptOut={true}
      />
    );
  }
}

MovieIndexTable.propTypes = {
  items: PropTypes.arrayOf(PropTypes.object).isRequired,
  columns: PropTypes.arrayOf(PropTypes.object).isRequired,
  filters: PropTypes.arrayOf(PropTypes.object).isRequired,
  sortKey: PropTypes.string,
  sortDirection: PropTypes.oneOf(sortDirections.all),
  scrollTop: PropTypes.number.isRequired,
  jumpToCharacter: PropTypes.string,
  contentBody: PropTypes.object.isRequired,
  isSmallScreen: PropTypes.bool.isRequired,
  onSortPress: PropTypes.func.isRequired,
  onRender: PropTypes.func.isRequired,
  onScroll: PropTypes.func.isRequired,
  allSelected: PropTypes.bool.isRequired,
  allUnselected: PropTypes.bool.isRequired,
  selectedState: PropTypes.object.isRequired,
  onSelectedChange: PropTypes.func.isRequired,
  onSelectAllChange: PropTypes.func.isRequired,
  isMovieEditorActive: PropTypes.bool.isRequired
};

export default MovieIndexTable;
