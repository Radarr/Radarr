import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { clearPendingChanges } from 'Store/Actions/baseActions';
import { cancelSaveRootFolder } from 'Store/Actions/settingsActions';
import EditRootFolderModal from './EditRootFolderModal';

function createMapDispatchToProps(dispatch, props) {
  const section = 'settings.rootFolders';

  return {
    dispatchClearPendingChanges() {
      dispatch(clearPendingChanges({ section }));
    },

    dispatchCancelSaveRootFolder() {
      dispatch(cancelSaveRootFolder({ section }));
    }
  };
}

class EditRootFolderModalConnector extends Component {

  //
  // Listeners

  onModalClose = () => {
    this.props.dispatchClearPendingChanges();
    this.props.dispatchCancelSaveRootFolder();
    this.props.onModalClose();
  }

  //
  // Render

  render() {
    const {
      dispatchClearPendingChanges,
      dispatchCancelSaveRootFolder,
      ...otherProps
    } = this.props;

    return (
      <EditRootFolderModal
        {...otherProps}
        onModalClose={this.onModalClose}
      />
    );
  }
}

EditRootFolderModalConnector.propTypes = {
  onModalClose: PropTypes.func.isRequired,
  dispatchClearPendingChanges: PropTypes.func.isRequired,
  dispatchCancelSaveRootFolder: PropTypes.func.isRequired
};

export default connect(null, createMapDispatchToProps)(EditRootFolderModalConnector);
