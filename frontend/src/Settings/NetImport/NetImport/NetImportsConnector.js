import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { fetchNetImports, deleteNetImport } from 'Store/Actions/settingsActions';
import { fetchRootFolders } from 'Store/Actions/rootFolderActions';
import NetImports from './NetImports';

function createMapStateToProps() {
  return createSelector(
    (state) => state.settings.netImports,
    (netImports) => {
      return {
        ...netImports
      };
    }
  );
}

const mapDispatchToProps = {
  fetchNetImports,
  deleteNetImport,
  fetchRootFolders
};

class NetImportsConnector extends Component {

  //
  // Lifecycle

  componentDidMount() {
    this.props.fetchNetImports();
    this.props.fetchRootFolders();
  }

  //
  // Listeners

  onConfirmDeleteNetImport = (id) => {
    this.props.deleteNetImport({ id });
  }

  //
  // Render

  render() {
    return (
      <NetImports
        {...this.props}
        onConfirmDeleteNetImport={this.onConfirmDeleteNetImport}
      />
    );
  }
}

NetImportsConnector.propTypes = {
  fetchNetImports: PropTypes.func.isRequired,
  deleteNetImport: PropTypes.func.isRequired,
  fetchRootFolders: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(NetImportsConnector);
