import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import createCommandExecutingSelector from 'Store/Selectors/createCommandExecutingSelector';
import { fetchBackups, deleteBackup } from 'Store/Actions/systemActions';
import { executeCommand } from 'Store/Actions/commandActions';
import * as commandNames from 'Commands/commandNames';
import Backups from './Backups';

function createMapStateToProps() {
  return createSelector(
    (state) => state.system.backups,
    createCommandExecutingSelector(commandNames.BACKUP),
    (backups, backupExecuting) => {
      const {
        isFetching,
        isPopulated,
        error,
        items
      } = backups;

      return {
        isFetching,
        isPopulated,
        error,
        items,
        backupExecuting
      };
    }
  );
}

function createMapDispatchToProps(dispatch, props) {
  return {
    dispatchFetchBackups() {
      dispatch(fetchBackups());
    },

    onDeleteBackupPress(id) {
      dispatch(deleteBackup({ id }));
    },

    onBackupPress() {
      dispatch(executeCommand({
        name: commandNames.BACKUP
      }));
    }
  };
}

class BackupsConnector extends Component {

  //
  // Lifecycle

  componentDidMount() {
    this.props.dispatchFetchBackups();
  }

  componentDidUpdate(prevProps) {
    if (prevProps.backupExecuting && !this.props.backupExecuting) {
      this.props.dispatchFetchBackups();
    }
  }

  //
  // Render

  render() {
    return (
      <Backups
        {...this.props}
      />
    );
  }
}

BackupsConnector.propTypes = {
  backupExecuting: PropTypes.bool.isRequired,
  dispatchFetchBackups: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, createMapDispatchToProps)(BackupsConnector);
