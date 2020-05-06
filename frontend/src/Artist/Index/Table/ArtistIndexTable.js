import PropTypes from 'prop-types';
import React, { Component } from 'react';
import getIndexOfFirstCharacter from 'Utilities/Array/getIndexOfFirstCharacter';
import { sortDirections } from 'Helpers/Props';
import VirtualTable from 'Components/Table/VirtualTable';
import VirtualTableRow from 'Components/Table/VirtualTableRow';
import ArtistIndexItemConnector from 'Artist/Index/ArtistIndexItemConnector';
import ArtistIndexHeaderConnector from './ArtistIndexHeaderConnector';
import ArtistIndexRow from './ArtistIndexRow';
import styles from './ArtistIndexTable.css';

class ArtistIndexTable extends Component {

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
      showBanners
    } = this.props;

    const artist = items[rowIndex];

    return (
      <VirtualTableRow
        key={key}
        style={style}
      >
        <ArtistIndexItemConnector
          key={artist.id}
          component={ArtistIndexRow}
          style={style}
          columns={columns}
          authorId={artist.id}
          qualityProfileId={artist.qualityProfileId}
          metadataProfileId={artist.metadataProfileId}
          showBanners={showBanners}
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
      sortKey,
      sortDirection,
      showBanners,
      isSmallScreen,
      onSortPress,
      scroller
    } = this.props;

    return (
      <VirtualTable
        className={styles.tableContainer}
        items={items}
        scrollIndex={this.state.scrollIndex}
        isSmallScreen={isSmallScreen}
        scroller={scroller}
        rowHeight={showBanners ? 70 : 38}
        overscanRowCount={2}
        rowRenderer={this.rowRenderer}
        header={
          <ArtistIndexHeaderConnector
            showBanners={showBanners}
            columns={columns}
            sortKey={sortKey}
            sortDirection={sortDirection}
            onSortPress={onSortPress}
          />
        }
        columns={columns}
        sortKey={sortKey}
        sortDirection={sortDirection}
      />
    );
  }
}

ArtistIndexTable.propTypes = {
  items: PropTypes.arrayOf(PropTypes.object).isRequired,
  columns: PropTypes.arrayOf(PropTypes.object).isRequired,
  sortKey: PropTypes.string,
  sortDirection: PropTypes.oneOf(sortDirections.all),
  showBanners: PropTypes.bool.isRequired,
  jumpToCharacter: PropTypes.string,
  scroller: PropTypes.instanceOf(Element).isRequired,
  isSmallScreen: PropTypes.bool.isRequired,
  onSortPress: PropTypes.func.isRequired
};

export default ArtistIndexTable;
