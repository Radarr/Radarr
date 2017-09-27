import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { clearPendingChanges } from 'Store/Actions/baseActions';
import EditMetadataModal from './EditMetadataModal';

function createMapDispatchToProps(dispatch, props) {
  const section = 'metadata';

  return {
    dispatchClearPendingChanges() {
      dispatch(clearPendingChanges)({ section });
    }
  };
}

class EditMetadataModalConnector extends Component {

  //
  // Listeners

  onModalClose = () => {
    this.props.clearPendingChanges({ section: 'metadatas' });
    this.props.onModalClose();
  }

  //
  // Render

  render() {
    return (
      <EditMetadataModal
        {...this.props}
        onModalClose={this.onModalClose}
      />
    );
  }
}

EditMetadataModalConnector.propTypes = {
  onModalClose: PropTypes.func.isRequired,
  clearPendingChanges: PropTypes.func.isRequired
};

export default connect(null, createMapDispatchToProps)(EditMetadataModalConnector);
