import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import Table from 'Components/Table/Table';
import TableBody from 'Components/Table/TableBody';
import { sortDirections } from 'Helpers/Props';
import getToggledRange from 'Utilities/Table/getToggledRange';
import BookRowConnector from './BookRowConnector';
import styles from './AuthorDetailsSeason.css';

class AuthorDetailsSeason extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      lastToggledBook: null
    };
  }

  //
  // Listeners

  onMonitorBookPress = (bookId, monitored, { shiftKey }) => {
    const lastToggled = this.state.lastToggledBook;
    const bookIds = [bookId];

    if (shiftKey && lastToggled) {
      const { lower, upper } = getToggledRange(this.props.items, bookId, lastToggled);
      const items = this.props.items;

      for (let i = lower; i < upper; i++) {
        bookIds.push(items[i].id);
      }
    }

    this.setState({ lastToggledBook: bookId });

    this.props.onMonitorBookPress(_.uniq(bookIds), monitored);
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
        className={styles.bookType}
      >
        <div className={styles.books}>
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
                    <BookRowConnector
                      key={item.id}
                      columns={columns}
                      {...item}
                      onMonitorBookPress={this.onMonitorBookPress}
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

AuthorDetailsSeason.propTypes = {
  sortKey: PropTypes.string,
  sortDirection: PropTypes.oneOf(sortDirections.all),
  items: PropTypes.arrayOf(PropTypes.object).isRequired,
  columns: PropTypes.arrayOf(PropTypes.object).isRequired,
  onTableOptionChange: PropTypes.func.isRequired,
  onExpandPress: PropTypes.func.isRequired,
  onSortPress: PropTypes.func.isRequired,
  onMonitorBookPress: PropTypes.func.isRequired,
  uiSettings: PropTypes.object.isRequired
};

export default AuthorDetailsSeason;
