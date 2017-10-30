import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { clearPendingChanges } from 'Store/Actions/baseActions';
import EditArtistModal from './EditArtistModal';

const mapDispatchToProps = {
  clearPendingChanges
};

class EditArtistModalConnector extends Component {

  //
  // Listeners

  onModalClose = () => {
    this.props.clearPendingChanges({ section: 'artist' });
    this.props.onModalClose();
  }

  //
  // Render

  render() {
    return (
      <EditArtistModal
        {...this.props}
        onModalClose={this.onModalClose}
      />
    );
  }
}

EditArtistModalConnector.propTypes = {
  onModalClose: PropTypes.func.isRequired,
  clearPendingChanges: PropTypes.func.isRequired
};

export default connect(undefined, mapDispatchToProps)(EditArtistModalConnector);
