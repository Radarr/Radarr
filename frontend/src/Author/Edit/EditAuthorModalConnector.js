import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { clearPendingChanges } from 'Store/Actions/baseActions';
import EditAuthorModal from './EditAuthorModal';

const mapDispatchToProps = {
  clearPendingChanges
};

class EditAuthorModalConnector extends Component {

  //
  // Listeners

  onModalClose = () => {
    this.props.clearPendingChanges({ section: 'author' });
    this.props.onModalClose();
  }

  //
  // Render

  render() {
    return (
      <EditAuthorModal
        {...this.props}
        onModalClose={this.onModalClose}
      />
    );
  }
}

EditAuthorModalConnector.propTypes = {
  onModalClose: PropTypes.func.isRequired,
  clearPendingChanges: PropTypes.func.isRequired
};

export default connect(undefined, mapDispatchToProps)(EditAuthorModalConnector);
