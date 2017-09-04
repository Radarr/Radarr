import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { clearPendingChanges } from 'Store/Actions/baseActions';
import EditDownloadClientModal from './EditDownloadClientModal';

const mapDispatchToProps = {
  clearPendingChanges
};

class EditDownloadClientModalConnector extends Component {

  //
  // Listeners

  onModalClose = () => {
    this.props.clearPendingChanges({ section: 'downloadClients' });
    this.props.onModalClose();
  }

  //
  // Render

  render() {
    return (
      <EditDownloadClientModal
        {...this.props}
        onModalClose={this.onModalClose}
      />
    );
  }
}

EditDownloadClientModalConnector.propTypes = {
  onModalClose: PropTypes.func.isRequired,
  clearPendingChanges: PropTypes.func.isRequired
};

export default connect(null, mapDispatchToProps)(EditDownloadClientModalConnector);
