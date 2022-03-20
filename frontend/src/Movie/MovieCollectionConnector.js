import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { saveImportList, selectImportListSchema, setImportListFieldValue, setImportListValue } from 'Store/Actions/settingsActions';
import createMovieCollectionListSelector from 'Store/Selectors/createMovieCollectionListSelector';
import createMovieSelector from 'Store/Selectors/createMovieSelector';
import MovieCollection from './MovieCollection';

function createMapStateToProps() {
  return createSelector(
    createMovieSelector(),
    createMovieCollectionListSelector(),
    (state) => state.settings.importLists,
    (movie, collectionList, importLists) => {
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
        isSaving: importLists.isSaving
      };
    }
  );
}

const mapDispatchToProps = {
  selectImportListSchema,
  setImportListFieldValue,
  setImportListValue,
  saveImportList
};

class MovieCollectionConnector extends Component {

  //
  // Listeners

  onMonitorTogglePress = (monitored) => {
    if (this.props.collectionList) {
      this.props.setImportListValue({ name: 'enabled', value: monitored });
      this.props.setImportListValue({ name: 'enableAuto', value: monitored });
      this.props.saveImportList({ id: this.props.collectionList.id });
    } else {
      this.props.selectImportListSchema({ implementation: 'TMDbCollectionImport', presetName: undefined });
      this.props.setImportListFieldValue({ name: 'collectionId', value: this.props.tmdbId.toString() });
      this.props.setImportListValue({ name: 'enabled', value: true });
      this.props.setImportListValue({ name: 'enableAuto', value: true });
      this.props.setImportListValue({ name: 'name', value: `${this.props.name} - ${this.props.tmdbId}` });
      this.props.setImportListValue({ name: 'qualityProfileId', value: this.props.qualityProfileId });
      this.props.setImportListValue({ name: 'monitored', value: this.props.monitored });
      this.props.setImportListValue({ name: 'minimumAvailability', value: this.props.minimumAvailability });
    }
  };

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
  selectImportListSchema: PropTypes.func.isRequired,
  setImportListFieldValue: PropTypes.func.isRequired,
  setImportListValue: PropTypes.func.isRequired,
  saveImportList: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(MovieCollectionConnector);
