import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import createBookFileSelector from 'Store/Selectors/createBookFileSelector';
import createBookSelector from 'Store/Selectors/createBookSelector';
import createQueueItemSelector from 'Store/Selectors/createQueueItemSelector';
import EpisodeStatus from './EpisodeStatus';

function createMapStateToProps() {
  return createSelector(
    createBookSelector(),
    createQueueItemSelector(),
    createBookFileSelector(),
    (book, queueItem, bookFile) => {
      const result = _.pick(book, [
        'airDateUtc',
        'monitored',
        'grabbed'
      ]);

      result.queueItem = queueItem;
      result.bookFile = bookFile;

      return result;
    }
  );
}

const mapDispatchToProps = {
};

class EpisodeStatusConnector extends Component {

  //
  // Render

  render() {
    return (
      <EpisodeStatus
        {...this.props}
      />
    );
  }
}

EpisodeStatusConnector.propTypes = {
  bookId: PropTypes.number.isRequired,
  bookFileId: PropTypes.number.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(EpisodeStatusConnector);
