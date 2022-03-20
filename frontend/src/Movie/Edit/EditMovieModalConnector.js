import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { clearPendingChanges } from 'Store/Actions/baseActions';
import EditMovieModal from './EditMovieModal';

const mapDispatchToProps = {
  clearPendingChanges
};

class EditMovieModalConnector extends Component {

  //
  // Listeners

  onModalClose = () => {
    this.props.clearPendingChanges({ section: 'movies' });
    this.props.onModalClose();
  };

  //
  // Render

  render() {
    return (
      <EditMovieModal
        {...this.props}
        onModalClose={this.onModalClose}
      />
    );
  }
}

EditMovieModalConnector.propTypes = {
  onModalClose: PropTypes.func.isRequired,
  clearPendingChanges: PropTypes.func.isRequired
};

export default connect(undefined, mapDispatchToProps)(EditMovieModalConnector);
