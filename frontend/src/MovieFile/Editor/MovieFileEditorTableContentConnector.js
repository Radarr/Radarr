import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { deleteMovieFile, setMovieFilesSort, setMovieFilesTableOption } from 'Store/Actions/movieFileActions';
import { fetchLanguages, fetchQualityProfileSchema } from 'Store/Actions/settingsActions';
import createClientSideCollectionSelector from 'Store/Selectors/createClientSideCollectionSelector';
import createMovieSelector from 'Store/Selectors/createMovieSelector';
import getQualities from 'Utilities/Quality/getQualities';
import MovieFileEditorTableContent from './MovieFileEditorTableContent';

function createMapStateToProps() {
  return createSelector(
    (state, { movieId }) => movieId,
    createClientSideCollectionSelector('movieFiles'),
    (state) => state.settings.languages,
    (state) => state.settings.qualityProfiles,
    createMovieSelector(),
    (
      movieId,
      movieFiles,
      languageProfiles,
      qualityProfiles
    ) => {
      const languages = languageProfiles.items;
      const qualities = getQualities(qualityProfiles.schema.items);
      const filesForMovie = movieFiles.items.filter((file) => file.movieId === movieId);

      return {
        items: filesForMovie,
        columns: movieFiles.columns,
        sortKey: movieFiles.sortKey,
        sortDirection: movieFiles.sortDirection,
        isDeleting: movieFiles.isDeleting,
        isSaving: movieFiles.isSaving,
        error: null,
        languages,
        qualities
      };
    }
  );
}

const mapDispatchToProps = {
  fetchQualityProfileSchema,
  fetchLanguages,
  deleteMovieFile,
  setMovieFilesTableOption,
  setMovieFilesSort
};

class MovieFileEditorTableContentConnector extends Component {

  //
  // Lifecycle

  componentDidMount() {
    this.props.fetchLanguages();
    this.props.fetchQualityProfileSchema();
  }

  //
  // Listeners

  onDeletePress = (movieFileId) => {
    this.props.deleteMovieFile({
      id: movieFileId
    });
  };

  onTableOptionChange = (payload) => {
    this.props.setMovieFilesTableOption(payload);
  };

  onSortPress = (sortKey, sortDirection) => {
    this.props.setMovieFilesSort({
      sortKey,
      sortDirection
    });
  };

  //
  // Render

  render() {
    return (
      <MovieFileEditorTableContent
        {...this.props}
        onDeletePress={this.onDeletePress}
        onTableOptionChange={this.onTableOptionChange}
        onSortPress={this.onSortPress}
      />
    );
  }
}

MovieFileEditorTableContentConnector.propTypes = {
  movieId: PropTypes.number.isRequired,
  languages: PropTypes.arrayOf(PropTypes.object).isRequired,
  qualities: PropTypes.arrayOf(PropTypes.object).isRequired,
  fetchLanguages: PropTypes.func.isRequired,
  fetchQualityProfileSchema: PropTypes.func.isRequired,
  deleteMovieFile: PropTypes.func.isRequired,
  setMovieFilesTableOption: PropTypes.func.isRequired,
  setMovieFilesSort: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(MovieFileEditorTableContentConnector);
