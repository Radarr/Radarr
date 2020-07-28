import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { saveNetImport, selectNetImportSchema, setNetImportFieldValue, setNetImportValue } from 'Store/Actions/settingsActions';
import createMovieCollectionListSelector from 'Store/Selectors/createMovieCollectionListSelector';
import createMovieSelector from 'Store/Selectors/createMovieSelector';
import MovieCollection from './MovieCollection';

function createMapStateToProps() {
  return createSelector(
    createMovieSelector(),
    createMovieCollectionListSelector(),
    (state) => state.settings.netImports,
    (movie, collectionList, netImports) => {
      const {
        monitored,
        qualityProfileId,
        minimumAvailability
      } = movie;

      return {
        collectionList,
        monitored,
        qualityProfileId,
        minimumAvailability,
        isSaving: netImports.isSaving
      };
    }
  );
}

const mapDispatchToProps = {
  selectNetImportSchema,
  setNetImportFieldValue,
  setNetImportValue,
  saveNetImport
};

class MovieCollectionConnector extends Component {

  //
  // Listeners

  onMonitorTogglePress = (monitored) => {
    if (this.props.collectionList) {
      this.props.setNetImportValue({ name: 'enabled', value: monitored });
      this.props.setNetImportValue({ name: 'enableAuto', value: monitored });
      this.props.saveNetImport({ id: this.props.collectionList.id });
    } else {
      this.props.selectNetImportSchema({ implementation: 'TMDbCollectionImport', presetName: undefined });
      this.props.setNetImportFieldValue({ name: 'collectionId', value: this.props.tmdbId.toString() });
      this.props.setNetImportValue({ name: 'enabled', value: true });
      this.props.setNetImportValue({ name: 'enableAuto', value: true });
      this.props.setNetImportValue({ name: 'name', value: `${this.props.name} - ${this.props.tmdbId}` });
      this.props.setNetImportValue({ name: 'qualityProfileId', value: this.props.qualityProfileId });
      this.props.setNetImportValue({ name: 'monitored', value: this.props.monitored });
      this.props.setNetImportValue({ name: 'minimumAvailability', value: this.props.minimumAvailability });
    }
  }

  //
  // Render

  render() {
    return (
      <MovieCollection
        {...this.props}
        onMonitorTogglePress={this.onMonitorTogglePress}
      />
    );
  }
}

MovieCollectionConnector.propTypes = {
  tmdbId: PropTypes.number.isRequired,
  movieId: PropTypes.number.isRequired,
  name: PropTypes.string.isRequired,
  collectionList: PropTypes.object,
  monitored: PropTypes.bool.isRequired,
  qualityProfileId: PropTypes.number.isRequired,
  minimumAvailability: PropTypes.string.isRequired,
  isSaving: PropTypes.bool.isRequired,
  selectNetImportSchema: PropTypes.func.isRequired,
  setNetImportFieldValue: PropTypes.func.isRequired,
  setNetImportValue: PropTypes.func.isRequired,
  saveNetImport: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(MovieCollectionConnector);
