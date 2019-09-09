import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { clearRetagPreview } from 'Store/Actions/retagPreviewActions';
import RetagPreviewModal from './RetagPreviewModal';

const mapDispatchToProps = {
  clearRetagPreview
};

class RetagPreviewModalConnector extends Component {

  //
  // Listeners

  onModalClose = () => {
    this.props.clearRetagPreview();
    this.props.onModalClose();
  }

  //
  // Render

  render() {
    return (
      <RetagPreviewModal
        {...this.props}
        onModalClose={this.onModalClose}
      />
    );
  }
}

RetagPreviewModalConnector.propTypes = {
  clearRetagPreview: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default connect(undefined, mapDispatchToProps)(RetagPreviewModalConnector);
