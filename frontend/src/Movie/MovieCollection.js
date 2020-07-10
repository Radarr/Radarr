import PropTypes from 'prop-types';
import React, { Component } from 'react';
import MonitorToggleButton from 'Components/MonitorToggleButton';
import EditNetImportModalConnector from 'Settings/NetImport/NetImport/EditNetImportModalConnector';
import styles from './MovieCollection.css';

class MovieCollection extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      hasPosterError: false,
      isEditNetImportModalOpen: false
    };
  }

  onAddNetImportPress = (monitored) => {
    if (this.props.collectionList) {
      this.props.onMonitorTogglePress(monitored);
    } else {
      this.props.onMonitorTogglePress(monitored);
      this.setState({ isEditNetImportModalOpen: true });
    }
  }

  onEditNetImportModalClose = () => {
    this.setState({ isEditNetImportModalOpen: false });
  }

  render() {
    const {
      name,
      collectionList,
      isSaving
    } = this.props;

    const monitored = collectionList !== undefined && collectionList.enabled && collectionList.enableAuto;
    const netImportId = collectionList ? collectionList.id : 0;

    return (
      <div>
        <MonitorToggleButton
          className={styles.monitorToggleButton}
          monitored={monitored}
          isSaving={isSaving}
          size={15}
          onPress={this.onAddNetImportPress}
        />
        {name}
        <EditNetImportModalConnector
          id={netImportId}
          isOpen={this.state.isEditNetImportModalOpen}
          onModalClose={this.onEditNetImportModalClose}
          onDeleteNetImportPress={this.onDeleteNetImportPress}
        />
      </div>
    );
  }
}

MovieCollection.propTypes = {
  tmdbId: PropTypes.number.isRequired,
  name: PropTypes.string.isRequired,
  collectionList: PropTypes.object,
  isSaving: PropTypes.bool.isRequired,
  onMonitorTogglePress: PropTypes.func.isRequired
};

export default MovieCollection;
