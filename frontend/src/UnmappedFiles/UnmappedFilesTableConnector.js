import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { registerPagePopulator, unregisterPagePopulator } from 'Utilities/pagePopulator';
import createClientSideCollectionSelector from 'Store/Selectors/createClientSideCollectionSelector';
import createDimensionsSelector from 'Store/Selectors/createDimensionsSelector';
import { fetchTrackFiles, deleteTrackFile, setTrackFilesSort, setTrackFilesTableOption } from 'Store/Actions/trackFileActions';
import withCurrentPage from 'Components/withCurrentPage';
import UnmappedFilesTable from './UnmappedFilesTable';

function createMapStateToProps() {
  return createSelector(
    createClientSideCollectionSelector('trackFiles'),
    createDimensionsSelector(),
    (
      trackFiles,
      dimensionsState
    ) => {
      // trackFiles could pick up mapped entries via signalR so filter again here
      const {
        items,
        ...otherProps
      } = trackFiles;
      const unmappedFiles = _.filter(items, { albumId: 0 });
      return {
        items: unmappedFiles,
        ...otherProps,
        isSmallScreen: dimensionsState.isSmallScreen
      };
    }
  );
}

function createMapDispatchToProps(dispatch, props) {
  return {
    onTableOptionChange(payload) {
      dispatch(setTrackFilesTableOption(payload));
    },

    onSortPress(sortKey) {
      dispatch(setTrackFilesSort({ sortKey }));
    },

    fetchUnmappedFiles() {
      dispatch(fetchTrackFiles({ unmapped: true }));
    },

    deleteUnmappedFile(id) {
      dispatch(deleteTrackFile({ id }));
    }
  };
}

class UnmappedFilesTableConnector extends Component {

  //
  // Lifecycle

  componentDidMount() {
    registerPagePopulator(this.repopulate, ['trackFileUpdated']);

    this.repopulate();
  }

  componentWillUnmount() {
    unregisterPagePopulator(this.repopulate);
  }

  //
  // Control

  repopulate = () => {
    this.props.fetchUnmappedFiles();
  }

  //
  // Render

  render() {
    return (
      <UnmappedFilesTable
        {...this.props}
      />
    );
  }
}

UnmappedFilesTableConnector.propTypes = {
  isSmallScreen: PropTypes.bool.isRequired,
  onSortPress: PropTypes.func.isRequired,
  onTableOptionChange: PropTypes.func.isRequired,
  fetchUnmappedFiles: PropTypes.func.isRequired,
  deleteUnmappedFile: PropTypes.func.isRequired
};

export default withCurrentPage(
  connect(createMapStateToProps, createMapDispatchToProps)(UnmappedFilesTableConnector)
);
