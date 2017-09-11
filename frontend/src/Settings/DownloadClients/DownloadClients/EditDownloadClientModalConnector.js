import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { clearPendingChanges } from 'Store/Actions/baseActions';
import { cancelTestDownloadClient, cancelSaveDownloadClient } from 'Store/Actions/settingsActions';
import EditDownloadClientModal from './EditDownloadClientModal';

function createMapDispatchToProps(dispatch, props) {
  return {
    dispatchClearPendingChanges() {
      dispatch(clearPendingChanges);
    },

    dispatchCancelTestDownloadClient() {
      dispatch(cancelTestDownloadClient);
    },

    dispatchCancelSaveDownloadClient() {
      dispatch(cancelSaveDownloadClient);
    }
  };
}

class EditDownloadClientModalConnector extends Component {

  //
  // Listeners

  onModalClose = () => {
    this.props.dispatchClearPendingChanges({ section: 'downloadClients' });
    this.props.dispatchCancelTestDownloadClient({ section: 'downloadClients' });
    this.props.dispatchCancelSaveDownloadClient({ section: 'downloadClients' });
    this.props.onModalClose();
  }

  //
  // Render

  render() {
    const {
      dispatchClearPendingChanges,
      dispatchCancelTestDownloadClient,
      dispatchCancelSaveDownloadClient,
      ...otherProps
    } = this.props;

    return (
      <EditDownloadClientModal
        {...otherProps}
        onModalClose={this.onModalClose}
      />
    );
  }
}

EditDownloadClientModalConnector.propTypes = {
  onModalClose: PropTypes.func.isRequired,
  dispatchClearPendingChanges: PropTypes.func.isRequired,
  dispatchCancelTestDownloadClient: PropTypes.func.isRequired,
  dispatchCancelSaveDownloadClient: PropTypes.func.isRequired
};

export default connect(null, createMapDispatchToProps)(EditDownloadClientModalConnector);
