import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { deleteMovieFile, setMovieFilesTableOption, updateMovieFiles } from 'Store/Actions/movieFileActions';
import { fetchLanguages, fetchQualityProfileSchema } from 'Store/Actions/settingsActions';
import createMovieSelector from 'Store/Selectors/createMovieSelector';
import getQualities from 'Utilities/Quality/getQualities';
import MovieFileEditorTableContent from './MovieFileEditorTableContent';

function createMapStateToProps() {
  return createSelector(
    (state, { movieId }) => movieId,
    (state) => state.movieFiles,
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
      const filesForMovie = movieFiles.items.filter((obj) => {
        return obj.movieId === movieId;
      });

      return {
        items: filesForMovie,
        columns: movieFiles.columns,
        isDeleting: movieFiles.isDeleting,
        isSaving: movieFiles.isSaving,
        error: null,
        languages,
        qualities
      };
    }
  );
}

function createMapDispatchToProps(dispatch, props) {
  return {
    dispatchFetchQualityProfileSchema(name, path) {
      dispatch(fetchQualityProfileSchema());
    },

    dispatchFetchLanguages(name, path) {
      dispatch(fetchLanguages());
    },

    dispatchUpdateMovieFiles(updateProps) {
      dispatch(updateMovieFiles(updateProps));
    },

    onTableOptionChange(payload) {
      dispatch(setMovieFilesTableOption(payload));
    },

    onDeletePress(movieFileId) {
      dispatch(deleteMovieFile({
        id: movieFileId
      }));
    }
  };
}

class MovieFileEditorTableContentConnector extends Component {

  //
  // Lifecycle

  componentDidMount() {
    this.props.dispatchFetchLanguages();
    this.props.dispatchFetchQualityProfileSchema();
  }

  //
  // Render

  render() {
    const {
      dispatchFetchLanguages,
      dispatchFetchQualityProfileSchema,
      dispatchUpdateMovieFiles,
      ...otherProps
    } = this.props;

    return (
      <MovieFileEditorTableContent
        {...otherProps}
      />
    );
  }
}

MovieFileEditorTableContentConnector.propTypes = {
  movieId: PropTypes.number.isRequired,
  languages: PropTypes.arrayOf(PropTypes.object).isRequired,
  qualities: PropTypes.arrayOf(PropTypes.object).isRequired,
  dispatchFetchLanguages: PropTypes.func.isRequired,
  dispatchFetchQualityProfileSchema: PropTypes.func.isRequired,
  dispatchUpdateMovieFiles: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, createMapDispatchToProps)(MovieFileEditorTableContentConnector);
