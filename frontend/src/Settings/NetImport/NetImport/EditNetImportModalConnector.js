import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { clearPendingChanges } from 'Store/Actions/baseActions';
import { cancelSaveNetImport, cancelTestNetImport } from 'Store/Actions/settingsActions';
import EditNetImportModal from './EditNetImportModal';

function createMapDispatchToProps(dispatch, props) {
  const section = 'settings.netImports';

  return {
    dispatchClearPendingChanges() {
      dispatch(clearPendingChanges({ section }));
    },

    dispatchCancelTestNetImport() {
      dispatch(cancelTestNetImport({ section }));
    },

    dispatchCancelSaveNetImport() {
      dispatch(cancelSaveNetImport({ section }));
    }
  };
}

class EditNetImportModalConnector extends Component {

  //
  // Listeners

  onModalClose = () => {
    this.props.dispatchClearPendingChanges();
    this.props.dispatchCancelTestNetImport();
    this.props.dispatchCancelSaveNetImport();
    this.props.onModalClose();
  }

  //
  // Render

  render() {
    const {
      dispatchClearPendingChanges,
      dispatchCancelTestNetImport,
      dispatchCancelSaveNetImport,
      ...otherProps
    } = this.props;

    return (
      <EditNetImportModal
        {...otherProps}
        onModalClose={this.onModalClose}
      />
    );
  }
}

EditNetImportModalConnector.propTypes = {
  onModalClose: PropTypes.func.isRequired,
  dispatchClearPendingChanges: PropTypes.func.isRequired,
  dispatchCancelTestNetImport: PropTypes.func.isRequired,
  dispatchCancelSaveNetImport: PropTypes.func.isRequired
};

export default connect(null, createMapDispatchToProps)(EditNetImportModalConnector);
