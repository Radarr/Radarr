import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import * as commandNames from 'Commands/commandNames';
import { saveAuthorEditor, setAuthorEditorFilter, setAuthorEditorSort, setAuthorEditorTableOption } from 'Store/Actions/authorEditorActions';
import { executeCommand } from 'Store/Actions/commandActions';
import { fetchRootFolders } from 'Store/Actions/settingsActions';
import createClientSideCollectionSelector from 'Store/Selectors/createClientSideCollectionSelector';
import createCommandExecutingSelector from 'Store/Selectors/createCommandExecutingSelector';
import AuthorEditor from './AuthorEditor';

function createMapStateToProps() {
  return createSelector(
    createClientSideCollectionSelector('authors', 'authorEditor'),
    createCommandExecutingSelector(commandNames.RENAME_AUTHOR),
    createCommandExecutingSelector(commandNames.RETAG_AUTHOR),
    (author, isOrganizingAuthor, isRetaggingAuthor) => {
      return {
        isOrganizingAuthor,
        isRetaggingAuthor,
        ...author
      };
    }
  );
}

const mapDispatchToProps = {
  dispatchSetAuthorEditorSort: setAuthorEditorSort,
  dispatchSetAuthorEditorFilter: setAuthorEditorFilter,
  dispatchSetAuthorEditorTableOption: setAuthorEditorTableOption,
  dispatchSaveAuthorEditor: saveAuthorEditor,
  dispatchFetchRootFolders: fetchRootFolders,
  dispatchExecuteCommand: executeCommand
};

class AuthorEditorConnector extends Component {

  //
  // Lifecycle

  componentDidMount() {
    this.props.dispatchFetchRootFolders();
  }

  //
  // Listeners

  onSortPress = (sortKey) => {
    this.props.dispatchSetAuthorEditorSort({ sortKey });
  }

  onFilterSelect = (selectedFilterKey) => {
    this.props.dispatchSetAuthorEditorFilter({ selectedFilterKey });
  }

  onTableOptionChange = (payload) => {
    this.props.dispatchSetAuthorEditorTableOption(payload);
  }

  onSaveSelected = (payload) => {
    this.props.dispatchSaveAuthorEditor(payload);
  }

  onMoveSelected = (payload) => {
    this.props.dispatchExecuteCommand({
      name: commandNames.MOVE_AUTHOR,
      ...payload
    });
  }

  //
  // Render

  render() {
    return (
      <AuthorEditor
        {...this.props}
        onSortPress={this.onSortPress}
        onFilterSelect={this.onFilterSelect}
        onSaveSelected={this.onSaveSelected}
        onTableOptionChange={this.onTableOptionChange}
      />
    );
  }
}

AuthorEditorConnector.propTypes = {
  dispatchSetAuthorEditorSort: PropTypes.func.isRequired,
  dispatchSetAuthorEditorFilter: PropTypes.func.isRequired,
  dispatchSetAuthorEditorTableOption: PropTypes.func.isRequired,
  dispatchSaveAuthorEditor: PropTypes.func.isRequired,
  dispatchFetchRootFolders: PropTypes.func.isRequired,
  dispatchExecuteCommand: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(AuthorEditorConnector);
