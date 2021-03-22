import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { fetchDelayProfiles, fetchImportLists, fetchIndexers, fetchNotifications, fetchRestrictions } from 'Store/Actions/settingsActions';
import { fetchTagDetails } from 'Store/Actions/tagActions';
import Tags from './Tags';

function createMapStateToProps() {
  return createSelector(
    (state) => state.tags,
    (tags) => {
      const isFetching = tags.isFetching || tags.details.isFetching;
      const error = tags.error || tags.details.error;
      const isPopulated = tags.isPopulated && tags.details.isPopulated;

      return {
        ...tags,
        isFetching,
        error,
        isPopulated
      };
    }
  );
}

const mapDispatchToProps = {
  dispatchFetchTagDetails: fetchTagDetails,
  dispatchFetchDelayProfiles: fetchDelayProfiles,
  dispatchFetchNotifications: fetchNotifications,
  dispatchFetchRestrictions: fetchRestrictions,
  dispatchFetchImportLists: fetchImportLists,
  dispatchFetchIndexers: fetchIndexers
};

class MetadatasConnector extends Component {

  //
  // Lifecycle

  componentDidMount() {
    const {
      dispatchFetchTagDetails,
      dispatchFetchDelayProfiles,
      dispatchFetchNotifications,
      dispatchFetchRestrictions,
      dispatchFetchImportLists,
      dispatchFetchIndexers
    } = this.props;

    dispatchFetchTagDetails();
    dispatchFetchDelayProfiles();
    dispatchFetchNotifications();
    dispatchFetchRestrictions();
    dispatchFetchImportLists();
    dispatchFetchIndexers();
  }

  //
  // Render

  render() {
    return (
      <Tags
        {...this.props}
      />
    );
  }
}

MetadatasConnector.propTypes = {
  dispatchFetchTagDetails: PropTypes.func.isRequired,
  dispatchFetchDelayProfiles: PropTypes.func.isRequired,
  dispatchFetchNotifications: PropTypes.func.isRequired,
  dispatchFetchRestrictions: PropTypes.func.isRequired,
  dispatchFetchImportLists: PropTypes.func.isRequired,
  dispatchFetchIndexers: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(MetadatasConnector);
