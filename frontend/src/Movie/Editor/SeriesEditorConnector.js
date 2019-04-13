import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import createClientSideCollectionSelector from 'Store/Selectors/createClientSideCollectionSelector';
import createCommandExecutingSelector from 'Store/Selectors/createCommandExecutingSelector';
import { setSeriesEditorSort, setSeriesEditorFilter, saveSeriesEditor } from 'Store/Actions/movieEditorActions';
import { fetchRootFolders } from 'Store/Actions/rootFolderActions';
import { executeCommand } from 'Store/Actions/commandActions';
import * as commandNames from 'Commands/commandNames';
import SeriesEditor from './SeriesEditor';

function createMapStateToProps() {
  return createSelector(
    createClientSideCollectionSelector('movies', 'movieEditor'),
    createCommandExecutingSelector(commandNames.RENAME_SERIES),
    (series, isOrganizingSeries) => {
      return {
        isOrganizingSeries,
        ...series
      };
    }
  );
}

const mapDispatchToProps = {
  dispatchSetSeriesEditorSort: setSeriesEditorSort,
  dispatchSetSeriesEditorFilter: setSeriesEditorFilter,
  dispatchSaveMovieEditor: saveSeriesEditor,
  dispatchFetchRootFolders: fetchRootFolders,
  dispatchExecuteCommand: executeCommand
};

class SeriesEditorConnector extends Component {

  //
  // Lifecycle

  componentDidMount() {
    this.props.dispatchFetchRootFolders();
  }

  //
  // Listeners

  onSortPress = (sortKey) => {
    this.props.dispatchSetSeriesEditorSort({ sortKey });
  }

  onFilterSelect = (selectedFilterKey) => {
    this.props.dispatchSetSeriesEditorFilter({ selectedFilterKey });
  }

  onSaveSelected = (payload) => {
    this.props.dispatchSaveMovieEditor(payload);
  }

  onMoveSelected = (payload) => {
    this.props.dispatchExecuteCommand({
      name: commandNames.MOVE_SERIES,
      ...payload
    });
  }

  //
  // Render

  render() {
    return (
      <SeriesEditor
        {...this.props}
        onSortPress={this.onSortPress}
        onFilterSelect={this.onFilterSelect}
        onSaveSelected={this.onSaveSelected}
      />
    );
  }
}

SeriesEditorConnector.propTypes = {
  dispatchSetSeriesEditorSort: PropTypes.func.isRequired,
  dispatchSetSeriesEditorFilter: PropTypes.func.isRequired,
  dispatchSaveMovieEditor: PropTypes.func.isRequired,
  dispatchFetchRootFolders: PropTypes.func.isRequired,
  dispatchExecuteCommand: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(SeriesEditorConnector);
