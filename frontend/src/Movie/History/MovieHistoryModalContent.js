import PropTypes from 'prop-types';
import React, { Component } from 'react';
import Button from 'Components/Link/Button';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import ModalContent from 'Components/Modal/ModalContent';
import ModalHeader from 'Components/Modal/ModalHeader';
import ModalBody from 'Components/Modal/ModalBody';
import ModalFooter from 'Components/Modal/ModalFooter';
import Table from 'Components/Table/Table';
import TableBody from 'Components/Table/TableBody';
import MovieHistoryRowConnector from './MovieHistoryRowConnector';
const columns = [
  {
    name: 'eventType',
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

class MovieHistoryModalContent extends Component {

  //
  // Render

  render() {
    const {
      isFetching,
      isPopulated,
      error,
      items,
      onMarkAsFailedPress,
      onModalClose
    } = this.props;

    const hasItems = !!items.length;

    return (    
      <div>
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
                      <MovieHistoryRowConnector
                        key={item.id}
                        {...item}
                        onMarkAsFailedPress={onMarkAsFailedPress}
                      />
                    );
                  })
                }
              </TableBody>
            </Table>
          }
      </div>
    );
  }
}

MovieHistoryModalContent.propTypes = {
  isFetching: PropTypes.bool.isRequired,
  isPopulated: PropTypes.bool.isRequired,
  error: PropTypes.object,
  items: PropTypes.arrayOf(PropTypes.object).isRequired,
  onMarkAsFailedPress: PropTypes.func.isRequired
};

export default MovieHistoryModalContent;
