import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import MonitorToggleButton from 'Components/MonitorToggleButton';
import getToggledRange from 'Utilities/Table/getToggledRange';
import { icons, sortDirections } from 'Helpers/Props';
import Icon from 'Components/Icon';
import IconButton from 'Components/Link/IconButton';
import Link from 'Components/Link/Link';
import Table from 'Components/Table/Table';
import TableBody from 'Components/Table/TableBody';
import BookRowConnector from './BookRowConnector';
import styles from './AuthorDetailsSeries.css';

class AuthorDetailsSeries extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      isOrganizeModalOpen: false,
      isManageBooksOpen: false,
      lastToggledBook: null
    };
  }

  componentDidMount() {
    this._expandByDefault();
  }

  componentDidUpdate(prevProps) {
    const {
      authorId
    } = this.props;

    if (prevProps.authorId !== authorId) {
      this._expandByDefault();
      return;
    }
  }

  //
  // Control

  _expandByDefault() {
    const {
      id,
      onExpandPress
    } = this.props;

    onExpandPress(id, true);
  }

  isSeriesMonitored(series) {
    return series.items.every((book) => book.monitored);
  }

  isSeriesSaving(series) {
    return series.items.some((book) => book.isSaving);
  }

  //
  // Listeners

  onExpandPress = () => {
    const {
      id,
      isExpanded
    } = this.props;

    this.props.onExpandPress(id, !isExpanded);
  }

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

  onMonitorSeriesPress = (monitored, { shiftKey }) => {
    const bookIds = this.props.items.map((book) => book.id);

    this.props.onMonitorBookPress(_.uniq(bookIds), monitored);
  }

  //
  // Render

  render() {
    const {
      label,
      items,
      positionMap,
      columns,
      isExpanded,
      sortKey,
      sortDirection,
      onSortPress,
      isSmallScreen,
      onTableOptionChange,
      authorMonitored
    } = this.props;

    return (
      <div
        className={styles.bookType}
      >
        <div className={styles.seriesTitle}>
          <MonitorToggleButton
            size={24}
            monitored={this.isSeriesMonitored(this.props)}
            isDisabled={!authorMonitored}
            isSaving={this.isSeriesSaving(this.props)}
            onPress={this.onMonitorSeriesPress}
          />

          <Link
            className={styles.expandButton}
            onPress={this.onExpandPress}
          >
            <div className={styles.header}>
              <div className={styles.left}>
                {
                  <div>
                    <span className={styles.bookTypeLabel}>
                      {label}
                    </span>

                    <span className={styles.bookCount}>
                      ({items.length} Books)
                    </span>
                  </div>
                }

              </div>

              <Icon
                className={styles.expandButtonIcon}
                name={isExpanded ? icons.COLLAPSE : icons.EXPAND}
                title={isExpanded ? 'Hide books' : 'Show books'}
                size={24}
              />

              {
                !isSmallScreen &&
                  <span>&nbsp;</span>
              }

            </div>
          </Link>
        </div>

        <div>
          {
            isExpanded &&
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
                            position={positionMap[item.id]}
                            onMonitorBookPress={this.onMonitorBookPress}
                          />
                        );
                      })
                    }
                  </TableBody>
                </Table>

                <div className={styles.collapseButtonContainer}>
                  <IconButton
                    iconClassName={styles.collapseButtonIcon}
                    name={icons.COLLAPSE}
                    size={20}
                    title="Hide books"
                    onPress={this.onExpandPress}
                  />
                </div>
              </div>
          }
        </div>
      </div>
    );
  }
}

AuthorDetailsSeries.propTypes = {
  id: PropTypes.number.isRequired,
  authorId: PropTypes.number.isRequired,
  label: PropTypes.string.isRequired,
  sortKey: PropTypes.string,
  sortDirection: PropTypes.oneOf(sortDirections.all),
  items: PropTypes.arrayOf(PropTypes.object).isRequired,
  positionMap: PropTypes.object.isRequired,
  columns: PropTypes.arrayOf(PropTypes.object).isRequired,
  isExpanded: PropTypes.bool,
  isSmallScreen: PropTypes.bool.isRequired,
  onTableOptionChange: PropTypes.func.isRequired,
  onExpandPress: PropTypes.func.isRequired,
  onSortPress: PropTypes.func.isRequired,
  onMonitorBookPress: PropTypes.func.isRequired,
  uiSettings: PropTypes.object.isRequired,
  authorMonitored: PropTypes.object.isRequired
};

export default AuthorDetailsSeries;
