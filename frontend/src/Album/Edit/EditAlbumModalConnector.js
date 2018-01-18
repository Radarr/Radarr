import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { clearPendingChanges } from 'Store/Actions/baseActions';
import EditAlbumModal from './EditAlbumModal';

const mapDispatchToProps = {
  clearPendingChanges
};

class EditAlbumModalConnector extends Component {

  //
  // Listeners

  onModalClose = () => {
    this.props.clearPendingChanges({ section: 'albums' });
    this.props.onModalClose();
  }

  //
  // Render

  render() {
    return (
      <EditAlbumModal
        {...this.props}
        onModalClose={this.onModalClose}
      />
    );
  }
}

EditAlbumModalConnector.propTypes = {
  onModalClose: PropTypes.func.isRequired,
  clearPendingChanges: PropTypes.func.isRequired
};

export default connect(undefined, mapDispatchToProps)(EditAlbumModalConnector);
