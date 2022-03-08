import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { clearPendingChanges } from 'Store/Actions/baseActions';
import EditCollectionModal from './EditCollectionModal';

const mapDispatchToProps = {
  clearPendingChanges
};

class EditCollectionModalConnector extends Component {

  //
  // Listeners

  onModalClose = () => {
    this.props.clearPendingChanges({ section: 'movieCollections' });
    this.props.onModalClose();
  };

  //
  // Render

  render() {
    return (
      <EditCollectionModal
        {...this.props}
        onModalClose={this.onModalClose}
      />
    );
  }
}

EditCollectionModalConnector.propTypes = {
  onModalClose: PropTypes.func.isRequired,
  clearPendingChanges: PropTypes.func.isRequired
};

export default connect(undefined, mapDispatchToProps)(EditCollectionModalConnector);
