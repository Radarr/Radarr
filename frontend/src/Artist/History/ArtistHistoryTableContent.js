import PropTypes from 'prop-types';
import React, { Component } from 'react';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import Table from 'Components/Table/Table';
import TableBody from 'Components/Table/TableBody';
import ArtistHistoryRowConnector from './ArtistHistoryRowConnector';

const columns = [
  {
    name: 'eventType',
    isVisible: true
  },
  {
    name: 'album',
    label: 'Album',
    isVisible: true
  },
  {
    name: 'sourceTitle',
    label: 'Source Title',
    isVisible: true
  },
  {
    name: 'quality',
    label: 'Quality',
    isVisible: true
  },
  {
    name: 'date',
    label: 'Date',
    isVisible: true
  },
  {
    name: 'details',
    label: 'Details',
    isVisible: true
  },
  {
    name: 'actions',
    label: 'Actions',
    isVisible: true
  }
];

class ArtistHistoryTableContent extends Component {

  //
  // Render

  render() {
    const {
      bookId,
      isFetching,
      isPopulated,
      error,
      items,
      onMarkAsFailedPress
    } = this.props;

    const fullArtist = bookId == null;
    const hasItems = !!items.length;

    return (
      <>
        {
          isFetching &&
            <LoadingIndicator />
        }

        {
          !isFetching && !!error &&
            <div>Unable to load history.</div>
        }

        {
          isPopulated && !hasItems && !error &&
            <div>No history.</div>
        }

        {
          isPopulated && hasItems && !error &&
            <Table columns={columns}>
              <TableBody>
                {
                  items.map((item) => {
                    return (
                      <ArtistHistoryRowConnector
                        key={item.id}
                        fullArtist={fullArtist}
                        {...item}
                        onMarkAsFailedPress={onMarkAsFailedPress}
                      />
                    );
                  })
                }
              </TableBody>
            </Table>
        }
      </>
    );
  }
}

ArtistHistoryTableContent.propTypes = {
  bookId: PropTypes.number,
  isFetching: PropTypes.bool.isRequired,
  isPopulated: PropTypes.bool.isRequired,
  error: PropTypes.object,
  items: PropTypes.arrayOf(PropTypes.object).isRequired,
  onMarkAsFailedPress: PropTypes.func.isRequired
};

export default ArtistHistoryTableContent;
