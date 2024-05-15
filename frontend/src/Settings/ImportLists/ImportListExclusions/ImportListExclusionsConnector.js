import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import {
  deleteImportExclusion,
  fetchImportExclusions,
  purgeImportExclusions
} from 'Store/Actions/settingsActions';
import ImportListExclusions from './ImportListExclusions';

function createMapStateToProps() {
  return createSelector(
    (state) => state.settings.importExclusions,
    (importExclusions) => {
      return {
        ...importExclusions
      };
    }
  );
}

const mapDispatchToProps = {
  fetchImportExclusions,
  deleteImportExclusion,
  purgeImportExclusions
};

class ImportExclusionsConnector extends Component {
  //
  // Lifecycle

  componentDidMount() {
    this.props.fetchImportExclusions();
  }

  //
  // Listeners

  onConfirmDeleteImportExclusion = (id) => {
    this.props.deleteImportExclusion({ id });
  };

  onConfirmPurgeImportExclusions = () => {
    this.props.purgeImportExclusions();
  };

  //
  // Render

  render() {
    return (
      <ImportListExclusions
        {...this.state}
        {...this.props}
        onConfirmDeleteImportExclusion={this.onConfirmDeleteImportExclusion}
        onConfirmPurgeImportExclusions={this.onConfirmPurgeImportExclusions}
      />
    );
  }
}

ImportExclusionsConnector.propTypes = {
  fetchImportExclusions: PropTypes.func.isRequired,
  deleteImportExclusion: PropTypes.func.isRequired,
  purgeImportExclusions: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(ImportExclusionsConnector);
