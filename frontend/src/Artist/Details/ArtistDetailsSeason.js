import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import getToggledRange from 'Utilities/Table/getToggledRange';
import { sortDirections } from 'Helpers/Props';
import Table from 'Components/Table/Table';
import TableBody from 'Components/Table/TableBody';
import AlbumRowConnector from './AlbumRowConnector';
import styles from './ArtistDetailsSeason.css';

class ArtistDetailsSeason extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      lastToggledAlbum: null
    };
  }

  //
  // Listeners

  onMonitorAlbumPress = (bookId, monitored, { shiftKey }) => {
    const lastToggled = this.state.lastToggledAlbum;
    const bookIds = [bookId];

    if (shiftKey && lastToggled) {
      const { lower, upper } = getToggledRange(this.props.items, bookId, lastToggled);
      const items = this.props.items;

      for (let i = lower; i < upper; i++) {
        bookIds.push(items[i].id);
      }
    }

    this.setState({ lastToggledAlbum: bookId });

    this.props.onMonitorAlbumPress(_.uniq(bookIds), monitored);
  }

  //
  // Render

  render() {
    const {
      items,
      columns,
      sortKey,
      sortDirection,
      onSortPress,
      onTableOptionChange
    } = this.props;

    return (
      <div
        className={styles.albumType}
      >
        <div className={styles.albums}>
          <Table
            columns={columns}
            sortKey={sortKey}
            sortDirection={sortDirection}
            onSortPress={onSortPress}
            onTableOptionChange={onTableOptionChange}
          >
            <TableBody>
              {
                items.map((item) => {
                  return (
                    <AlbumRowConnector
                      key={item.id}
                      columns={columns}
                      {...item}
                      onMonitorAlbumPress={this.onMonitorAlbumPress}
                    />
                  );
                })
              }
            </TableBody>
          </Table>
        </div>
      </div>
    );
  }
}

ArtistDetailsSeason.propTypes = {
  sortKey: PropTypes.string,
  sortDirection: PropTypes.oneOf(sortDirections.all),
  items: PropTypes.arrayOf(PropTypes.object).isRequired,
  columns: PropTypes.arrayOf(PropTypes.object).isRequired,
  onTableOptionChange: PropTypes.func.isRequired,
  onExpandPress: PropTypes.func.isRequired,
  onSortPress: PropTypes.func.isRequired,
  onMonitorAlbumPress: PropTypes.func.isRequired,
  uiSettings: PropTypes.object.isRequired
};

export default ArtistDetailsSeason;
