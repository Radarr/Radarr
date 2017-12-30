import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { sortDirections } from 'Helpers/Props';
import VirtualTable from 'Components/Table/VirtualTable';
import ArtistIndexItemConnector from 'Artist/Index/ArtistIndexItemConnector';
import ArtistIndexHeaderConnector from './ArtistIndexHeaderConnector';
import ArtistIndexRow from './ArtistIndexRow';
import styles from './ArtistIndexTable.css';

class ArtistIndexTable extends Component {

  //
  // Control

  scrollToFirstCharacter(character) {
    const items = this.props.items;

    const row = _.findIndex(items, (item) => {
      const firstCharacter = item.sortName.charAt(0);

      if (character === '#') {
        return !isNaN(firstCharacter);
      }

      return firstCharacter === character;
    });

    if (row != null) {
      this._table.scrollToRow(row);
    }
  }

  rowRenderer = ({ key, rowIndex, style }) => {
    const {
      items,
      columns
    } = this.props;

    const artist = items[rowIndex];

    return (
      <ArtistIndexItemConnector
        key={key}
        component={ArtistIndexRow}
        style={style}
        columns={columns}
        artistId={artist.id}
        languageProfileId={artist.languageProfileId}
        qualityProfileId={artist.qualityProfileId}
        metadataProfileId={artist.metadataProfileId}
      />
    );
  }

  //
  // Render

  render() {
    const {
      items,
      columns,
      filterKey,
      filterValue,
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
        contentBody={contentBody}
        isSmallScreen={isSmallScreen}
        rowHeight={38}
        overscanRowCount={2}
        rowRenderer={this.rowRenderer}
        header={
          <ArtistIndexHeaderConnector
            columns={columns}
            sortKey={sortKey}
            sortDirection={sortDirection}
            onSortPress={onSortPress}
          />
        }
        columns={columns}
        filterKey={filterKey}
        filterValue={filterValue}
        sortKey={sortKey}
        sortDirection={sortDirection}
        onRender={onRender}
        onScroll={onScroll}
      />
    );
  }
}

ArtistIndexTable.propTypes = {
  items: PropTypes.arrayOf(PropTypes.object).isRequired,
  columns: PropTypes.arrayOf(PropTypes.object).isRequired,
  filterKey: PropTypes.string,
  filterValue: PropTypes.oneOfType([PropTypes.bool, PropTypes.number, PropTypes.string]),
  sortKey: PropTypes.string,
  sortDirection: PropTypes.oneOf(sortDirections.all),
  scrollTop: PropTypes.number.isRequired,
  contentBody: PropTypes.object.isRequired,
  isSmallScreen: PropTypes.bool.isRequired,
  onSortPress: PropTypes.func.isRequired,
  onRender: PropTypes.func.isRequired,
  onScroll: PropTypes.func.isRequired
};

export default ArtistIndexTable;
