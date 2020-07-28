import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { deleteNetImportExclusion, fetchNetImportExclusions } from 'Store/Actions/settingsActions';
import NetImportExclusions from './NetImportExclusions';

function createMapStateToProps() {
  return createSelector(
    (state) => state.settings.netImportExclusions,
    (netImportExclusions) => {
      return {
        ...netImportExclusions
      };
    }
  );
}

const mapDispatchToProps = {
  fetchNetImportExclusions,
  deleteNetImportExclusion
};

class NetImportExclusionsConnector extends Component {

  //
  // Lifecycle

  componentDidMount() {
    this.props.fetchNetImportExclusions();
  }

  //
  // Listeners

  onConfirmDeleteNetImportExclusion = (id) => {
    this.props.deleteNetImportExclusion({ id });
  }

  //
  // Render

  render() {
    return (
      <NetImportExclusions
        {...this.state}
        {...this.props}
        onConfirmDeleteNetImportExclusion={this.onConfirmDeleteNetImportExclusion}
      />
    );
  }
}

NetImportExclusionsConnector.propTypes = {
  fetchNetImportExclusions: PropTypes.func.isRequired,
  deleteNetImportExclusion: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(NetImportExclusionsConnector);
