import PropTypes from 'prop-types';
import React, { Component } from 'react';
import getIndexOfFirstCharacter from 'Utilities/Array/getIndexOfFirstCharacter';
import { sortDirections } from 'Helpers/Props';
import VirtualTable from 'Components/Table/VirtualTable';
import AddListMovieItemConnector from 'AddMovie/AddListMovie/AddListMovieItemConnector';
import AddListMovieHeaderConnector from './AddListMovieHeaderConnector';
import AddListMovieRowConnector from './AddListMovieRowConnector';
import styles from './AddListMovieTable.css';

class AddListMovieTable extends Component {

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
      columns
    } = this.props;

    const movie = items[rowIndex];

    return (
      <AddListMovieItemConnector
        key={key}
        component={AddListMovieRowConnector}
        style={style}
        columns={columns}
        movieId={movie.tmdbId}
      />
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
      onScroll
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
          <AddListMovieHeaderConnector
            columns={columns}
            sortKey={sortKey}
            sortDirection={sortDirection}
            onSortPress={onSortPress}
          />
        }
        columns={columns}
        filters={filters}
        sortKey={sortKey}
        sortDirection={sortDirection}
        onRender={onRender}
        onScroll={onScroll}
      />
    );
  }
}

AddListMovieTable.propTypes = {
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
  onScroll: PropTypes.func.isRequired
};

export default AddListMovieTable;
