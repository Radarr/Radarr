import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { setAddMovieDefault } from 'Store/Actions/discoverMovieActions';
import DiscoverMovieFooter from './DiscoverMovieFooter';

function createMapStateToProps() {
  return createSelector(
    (state) => state.discoverMovie,
    (state) => state.settings.importExclusions,
    (state, { selectedIds }) => selectedIds,
    (discoverMovie, importExclusions, selectedIds) => {
      const {
        monitor: defaultMonitor,
        qualityProfileId: defaultQualityProfileId,
        minimumAvailability: defaultMinimumAvailability,
        rootFolderPath: defaultRootFolderPath,
        searchForMovie: defaultSearchForMovie
      } = discoverMovie.defaults;

      const {
        isAdding
      } = discoverMovie;

      const {
        isSaving
      } = importExclusions;

      return {
        selectedCount: selectedIds.length,
        isAdding,
        isExcluding: isSaving,
        defaultMonitor,
        defaultQualityProfileId,
        defaultMinimumAvailability,
        defaultRootFolderPath,
        defaultSearchForMovie
      };
    }
  );
}

const mapDispatchToProps = {
  setAddMovieDefault
};

class DiscoverMovieFooterConnector extends Component {

  //
  // Listeners

  onInputChange = ({ name, value }) => {
    this.props.setAddMovieDefault({ [name]: value });
  };

  //
  // Render

  render() {
    return (
      <DiscoverMovieFooter
        {...this.props}
        onInputChange={this.onInputChange}
      />
    );
  }
}

DiscoverMovieFooterConnector.propTypes = {
  setAddMovieDefault: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(DiscoverMovieFooterConnector);
