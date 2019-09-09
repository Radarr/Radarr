import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { fetchUpdates } from 'Store/Actions/systemActions';
import { executeCommand } from 'Store/Actions/commandActions';
import createCommandExecutingSelector from 'Store/Selectors/createCommandExecutingSelector';
import createUISettingsSelector from 'Store/Selectors/createUISettingsSelector';
import createSystemStatusSelector from 'Store/Selectors/createSystemStatusSelector';
import * as commandNames from 'Commands/commandNames';
import Updates from './Updates';

function createMapStateToProps() {
  return createSelector(
    (state) => state.app.version,
    (state) => state.system.updates,
    createUISettingsSelector(),
    createCommandExecutingSelector(commandNames.APPLICATION_UPDATE),
    createSystemStatusSelector(),
    (
      currentVersion,
      updates,
      uiSettings,
      isInstallingUpdate,
      systemStatus
    ) => {
      const {
        isFetching,
        isPopulated,
        error,
        items
      } = updates;

      return {
        currentVersion,
        isFetching,
        isPopulated,
        error,
        items,
        isInstallingUpdate,
        isDocker: systemStatus.isDocker,
        shortDateFormat: uiSettings.shortDateFormat
      };
    }
  );
}

const mapDispatchToProps = {
  fetchUpdates,
  executeCommand
};

class UpdatesConnector extends Component {

  //
  // Lifecycle

  componentDidMount() {
    this.props.fetchUpdates();
  }

  //
  // Listeners

  onInstallLatestPress = () => {
    this.props.executeCommand({ name: commandNames.APPLICATION_UPDATE });
  }

  //
  // Render

  render() {
    return (
      <Updates
        onInstallLatestPress={this.onInstallLatestPress}
        {...this.props}
      />
    );
  }
}

UpdatesConnector.propTypes = {
  fetchUpdates: PropTypes.func.isRequired,
  executeCommand: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(UpdatesConnector);
