import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { clearPendingChanges } from 'Store/Actions/baseActions';
import EditMetadataProfileModal from './EditMetadataProfileModal';

function mapStateToProps() {
  return {};
}

const mapDispatchToProps = {
  clearPendingChanges
};

class EditMetadataProfileModalConnector extends Component {

  //
  // Listeners

  onModalClose = () => {
    this.props.clearPendingChanges({ section: 'settings.metadataProfiles' });
    this.props.onModalClose();
  }

  //
  // Render

  render() {
    return (
      <EditMetadataProfileModal
        {...this.props}
        onModalClose={this.onModalClose}
      />
    );
  }
}

EditMetadataProfileModalConnector.propTypes = {
  onModalClose: PropTypes.func.isRequired,
  clearPendingChanges: PropTypes.func.isRequired
};

export default connect(mapStateToProps, mapDispatchToProps)(EditMetadataProfileModalConnector);
