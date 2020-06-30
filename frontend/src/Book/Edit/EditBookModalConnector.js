import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { clearPendingChanges } from 'Store/Actions/baseActions';
import EditBookModal from './EditBookModal';

const mapDispatchToProps = {
  clearPendingChanges
};

class EditBookModalConnector extends Component {

  //
  // Listeners

  onModalClose = () => {
    this.props.clearPendingChanges({ section: 'books' });
    this.props.onModalClose();
  }

  //
  // Render

  render() {
    return (
      <EditBookModal
        {...this.props}
        onModalClose={this.onModalClose}
      />
    );
  }
}

EditBookModalConnector.propTypes = {
  onModalClose: PropTypes.func.isRequired,
  clearPendingChanges: PropTypes.func.isRequired
};

export default connect(undefined, mapDispatchToProps)(EditBookModalConnector);
