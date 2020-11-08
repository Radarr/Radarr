import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { clearPendingChanges } from 'Store/Actions/baseActions';
import EditImportExclusionModal from './EditImportExclusionModal';

function mapStateToProps() {
  return {};
}

const mapDispatchToProps = {
  clearPendingChanges
};

class EditImportExclusionModalConnector extends Component {

  //
  // Listeners

  onModalClose = () => {
    this.props.clearPendingChanges({ section: 'settings.importExclusions' });
    this.props.onModalClose();
  }

  //
  // Render

  render() {
    return (
      <EditImportExclusionModal
        {...this.props}
        onModalClose={this.onModalClose}
      />
    );
  }
}

EditImportExclusionModalConnector.propTypes = {
  onModalClose: PropTypes.func.isRequired,
  clearPendingChanges: PropTypes.func.isRequired
};

export default connect(mapStateToProps, mapDispatchToProps)(EditImportExclusionModalConnector);
