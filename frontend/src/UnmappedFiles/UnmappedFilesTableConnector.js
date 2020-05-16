import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { registerPagePopulator, unregisterPagePopulator } from 'Utilities/pagePopulator';
import createClientSideCollectionSelector from 'Store/Selectors/createClientSideCollectionSelector';
import createCommandExecutingSelector from 'Store/Selectors/createCommandExecutingSelector';
import createDimensionsSelector from 'Store/Selectors/createDimensionsSelector';
import { fetchBookFiles, deleteBookFile, setBookFilesSort, setBookFilesTableOption } from 'Store/Actions/bookFileActions';
import { executeCommand } from 'Store/Actions/commandActions';
import * as commandNames from 'Commands/commandNames';
import withCurrentPage from 'Components/withCurrentPage';
import UnmappedFilesTable from './UnmappedFilesTable';

function createMapStateToProps() {
  return createSelector(
    createClientSideCollectionSelector('bookFiles'),
    createCommandExecutingSelector(commandNames.RESCAN_FOLDERS),
    createDimensionsSelector(),
    (
      bookFiles,
      isScanningFolders,
      dimensionsState
    ) => {
      // bookFiles could pick up mapped entries via signalR so filter again here
      const {
        items,
        ...otherProps
      } = bookFiles;
      const unmappedFiles = _.filter(items, { bookId: 0 });
      return {
        items: unmappedFiles,
        ...otherProps,
        isScanningFolders,
        isSmallScreen: dimensionsState.isSmallScreen
      };
    }
  );
}

function createMapDispatchToProps(dispatch, props) {
  return {
    onTableOptionChange(payload) {
      dispatch(setBookFilesTableOption(payload));
    },

    onSortPress(sortKey) {
      dispatch(setBookFilesSort({ sortKey }));
    },

    fetchUnmappedFiles() {
      dispatch(fetchBookFiles({ unmapped: true }));
    },

    deleteUnmappedFile(id) {
      dispatch(deleteBookFile({ id }));
    },

    onAddMissingAuthorsPress() {
      dispatch(executeCommand({
        name: commandNames.RESCAN_FOLDERS,
        addNewAuthors: true,
        filter: 'matched'
      }));
    }
  };
}

class UnmappedFilesTableConnector extends Component {

  //
  // Lifecycle

  componentDidMount() {
    registerPagePopulator(this.repopulate, ['bookFileUpdated']);

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
