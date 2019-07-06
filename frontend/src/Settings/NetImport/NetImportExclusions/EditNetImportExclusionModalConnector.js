import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { clearPendingChanges } from 'Store/Actions/baseActions';
import EditNetImportExclusionModal from './EditNetImportExclusionModal';

function mapStateToProps() {
  return {};
}

const mapDispatchToProps = {
  clearPendingChanges
};

class EditNetImportExclusionModalConnector extends Component {

  //
  // Listeners

  onModalClose = () => {
    this.props.clearPendingChanges({ section: 'settings.netImportExclusions' });
    this.props.onModalClose();
  }

  //
  // Render

  render() {
    return (
      <EditNetImportExclusionModal
        {...this.props}
        onModalClose={this.onModalClose}
      />
    );
  }
}

EditNetImportExclusionModalConnector.propTypes = {
  onModalClose: PropTypes.func.isRequired,
  clearPendingChanges: PropTypes.func.isRequired
};

export default connect(mapStateToProps, mapDispatchToProps)(EditNetImportExclusionModalConnector);
