import PropTypes from 'prop-types';
import React, { Component } from 'react';
import BookSearchCellConnector from 'Book/BookSearchCellConnector';
import BookTitleLink from 'Book/BookTitleLink';
import Label from 'Components/Label';
import MonitorToggleButton from 'Components/MonitorToggleButton';
import StarRating from 'Components/StarRating';
import RelativeDateCellConnector from 'Components/Table/Cells/RelativeDateCellConnector';
import TableRowCell from 'Components/Table/Cells/TableRowCell';
import TableRow from 'Components/Table/TableRow';
import { kinds, sizes } from 'Helpers/Props';
import styles from './BookRow.css';

function getBookCountKind(monitored, bookFileCount, bookCount) {
  if (bookFileCount === bookCount && bookCount > 0) {
    return kinds.SUCCESS;
  }

  if (!monitored) {
    return kinds.WARNING;
  }

  return kinds.DANGER;
}

class BookRow extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      isDetailsModalOpen: false,
      isEditBookModalOpen: false
    };
  }

  //
  // Listeners

  onManualSearchPress = () => {
    this.setState({ isDetailsModalOpen: true });
  }

  onDetailsModalClose = () => {
    this.setState({ isDetailsModalOpen: false });
  }

  onEditBookPress = () => {
    this.setState({ isEditBookModalOpen: true });
  }

  onEditBookModalClose = () => {
    this.setState({ isEditBookModalOpen: false });
  }

  onMonitorBookPress = (monitored, options) => {
    this.props.onMonitorBookPress(this.props.id, monitored, options);
  }

  //
  // Render

  render() {
    const {
      id,
      authorId,
      monitored,
      statistics,
      releaseDate,
      title,
      seriesTitle,
      position,
      pageCount,
      ratings,
      isSaving,
      authorMonitored,
      titleSlug,
      columns
    } = this.props;

    const {
      bookCount,
      bookFileCount,
      totalBookCount
    } = statistics;

    return (
      <TableRow>
        {
          columns.map((column) => {
            const {
              name,
              isVisible
            } = column;

            if (!isVisible) {
              return null;
            }

            if (name === 'monitored') {
              return (
                <TableRowCell
                  key={name}
                  className={styles.monitored}
                >
                  <MonitorToggleButton
                    monitored={monitored}
                    isDisabled={!authorMonitored}
                    isSaving={isSaving}
                    onPress={this.onMonitorBookPress}
                  />
                </TableRowCell>
              );
            }

            if (name === 'title') {
              return (
                <TableRowCell
                  key={name}
                  className={styles.title}
                >
                  <BookTitleLink
                    titleSlug={titleSlug}
                    title={title}
                  />
                </TableRowCell>
              );
            }

            if (name === 'series') {
              return (
                <TableRowCell
                  key={name}
                  className={styles.title}
                >
                  {seriesTitle || ''}
                </TableRowCell>
              );
            }

            if (name === 'position') {
              return (
                <TableRowCell
                  key={name}
                  className={styles.title}
                >
                  {position || ''}
                </TableRowCell>
              );
            }

            if (name === 'rating') {
              return (
                <TableRowCell key={name}>
                  {
                    <StarRating
                      rating={ratings.value}
                      votes={ratings.votes}
                    />
                  }
                </TableRowCell>
              );
            }

            if (name === 'releaseDate') {
              return (
                <RelativeDateCellConnector
                  key={name}
                  date={releaseDate}
                />
              );
            }

            if (name === 'pageCount') {
              return (
                <TableRowCell
                  key={name}
                >
                  {pageCount || ''}
                </TableRowCell>
              );
            }

            if (name === 'status') {
              return (
                <TableRowCell
                  key={name}
                  className={styles.status}
                >
                  <Label
                    title={`${totalBookCount} books total. ${bookFileCount} books with files.`}
                    kind={getBookCountKind(monitored, bookFileCount, bookCount)}
                    size={sizes.MEDIUM}
                  >
                    {
                      <span>{bookFileCount} / {bookCount}</span>
                    }
                  </Label>
                </TableRowCell>
              );
            }

            if (name === 'actions') {
              return (
                <BookSearchCellConnector
                  key={name}
                  bookId={id}
                  authorId={authorId}
                  bookTitle={title}
                />
              );
            }
            return null;
          })
        }
      </TableRow>
    );
  }
}

BookRow.propTypes = {
  id: PropTypes.number.isRequired,
  authorId: PropTypes.number.isRequired,
  monitored: PropTypes.bool.isRequired,
  releaseDate: PropTypes.string,
  title: PropTypes.string.isRequired,
  seriesTitle: PropTypes.string.isRequired,
  position: PropTypes.number,
  pageCount: PropTypes.number,
  ratings: PropTypes.object.isRequired,
  titleSlug: PropTypes.string.isRequired,
  isSaving: PropTypes.bool,
  authorMonitored: PropTypes.bool.isRequired,
  statistics: PropTypes.object.isRequired,
  columns: PropTypes.arrayOf(PropTypes.object).isRequired,
  onMonitorBookPress: PropTypes.func.isRequired
};

BookRow.defaultProps = {
  statistics: {
    bookCount: 0,
    bookFileCount: 0
  }
};

export default BookRow;
