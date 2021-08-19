import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import InteractiveSearchRow from './InteractiveSearchRow';

function createMapStateToProps() {
  return createSelector(
    (state, { guid }) => guid,
    (state) => state.movieHistory.items,
    (state) => state.movieBlocklist.items,
    (guid, movieHistory, movieBlocklist) => {

      let blocklistData = {};
      let historyFailedData = {};

      const historyGrabbedData = movieHistory.find((movie) => movie.eventType === 'grabbed' && movie.data.guid === guid);
      if (historyGrabbedData) {
        historyFailedData = movieHistory.find((movie) => movie.eventType === 'downloadFailed' && movie.sourceTitle === historyGrabbedData.sourceTitle);
        blocklistData = movieBlocklist.find((item) => item.sourceTitle === historyGrabbedData.sourceTitle);
      }

      return {
        historyGrabbedData,
        historyFailedData,
        blocklistData
      };
    }
  );
}

class InteractiveSearchRowConnector extends Component {

  //
  // Render

  render() {
    const {
      historyGrabbedData,
      historyFailedData,
      blocklistData,
      ...otherProps
    } = this.props;

    return (
      <InteractiveSearchRow
        historyGrabbedData={historyGrabbedData}
        historyFailedData={historyFailedData}
        blocklistData={blocklistData}
        {...otherProps}
      />
    );
  }
}

InteractiveSearchRowConnector.propTypes = {
  historyGrabbedData: PropTypes.object,
  historyFailedData: PropTypes.object,
  blocklistData: PropTypes.object
};

export default connect(createMapStateToProps)(InteractiveSearchRowConnector);
