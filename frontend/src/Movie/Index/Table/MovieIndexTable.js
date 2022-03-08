import PropTypes from 'prop-types';
import React, { Component } from 'react';
import VirtualTable from 'Components/Table/VirtualTable';
import VirtualTableRow from 'Components/Table/VirtualTableRow';
import { sortDirections } from 'Helpers/Props';
import MovieIndexItemConnector from 'Movie/Index/MovieIndexItemConnector';
import getIndexOfFirstCharacter from 'Utilities/Array/getIndexOfFirstCharacter';
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
      items,
      jumpToCharacter
    } = this.props;

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
      isMovieEditorActive,
      movieRuntimeFormat
    } = this.props;

    const movie = items[rowIndex];

    return (
      <VirtualTableRow
        key={key}
        style={style}
      >
        <MovieIndexItemConnector
          key={movie.id}
          component={MovieIndexRow}
          columns={columns}
          movieId={movie.id}
          collectionId={movie.collectionId}
          qualityProfileId={movie.qualityProfileId}
          isSelected={selectedState[movie.id]}
          onSelectedChange={onSelectedChange}
          isMovieEditorActive={isMovieEditorActive}
          movieRuntimeFormat={movieRuntimeFormat}
        />
      </VirtualTableRow>
    );
  };

  //
  // Render

  render() {
    const {
      items,
      columns,
      sortKey,
      sortDirection,
      isSmallScreen,
      onSortPress,
      scroller,
      scrollTop,
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
        scrollIndex={this.state.scrollIndex}
        isSmallScreen={isSmallScreen}
        scrollTop={scrollTop}
        scroller={scroller}
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
      />
    );
  }
}

MovieIndexTable.propTypes = {
  items: PropTypes.arrayOf(PropTypes.object).isRequired,
  columns: PropTypes.arrayOf(PropTypes.object).isRequired,
  sortKey: PropTypes.string,
  sortDirection: PropTypes.oneOf(sortDirections.all),
  jumpToCharacter: PropTypes.string,
  isSmallScreen: PropTypes.bool.isRequired,
  scrollTop: PropTypes.number,
  scroller: PropTypes.instanceOf(Element).isRequired,
  onSortPress: PropTypes.func.isRequired,
  allSelected: PropTypes.bool.isRequired,
  allUnselected: PropTypes.bool.isRequired,
  selectedState: PropTypes.object.isRequired,
  onSelectedChange: PropTypes.func.isRequired,
  onSelectAllChange: PropTypes.func.isRequired,
  isMovieEditorActive: PropTypes.bool.isRequired,
  movieRuntimeFormat: PropTypes.string.isRequired
};

export default MovieIndexTable;
