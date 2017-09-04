import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { clearPendingChanges } from 'Store/Actions/baseActions';
import EditIndexerModal from './EditIndexerModal';

const mapDispatchToProps = {
  clearPendingChanges
};

class EditIndexerModalConnector extends Component {

  //
  // Listeners

  onModalClose = () => {
    this.props.clearPendingChanges({ section: 'indexers' });
    this.props.onModalClose();
  }

  //
  // Render

  render() {
    return (
      <EditIndexerModal
        {...this.props}
        onModalClose={this.onModalClose}
      />
    );
  }
}

EditIndexerModalConnector.propTypes = {
  onModalClose: PropTypes.func.isRequired,
  clearPendingChanges: PropTypes.func.isRequired
};

export default connect(null, mapDispatchToProps)(EditIndexerModalConnector);
