/* eslint max-params: 0 */
import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import getQualities from 'Utilities/Quality/getQualities';
import createMovieSelector from 'Store/Selectors/createMovieSelector';
import { deleteMovieFiles, updateMovieFiles } from 'Store/Actions/movieFileActions';
import { fetchQualityProfileSchema, fetchLanguages } from 'Store/Actions/settingsActions';
import MovieFileEditorTableContent from './MovieFileEditorTableContent';

function createMapStateToProps() {
  return createSelector(
    (state) => state.movieFiles,
    (state) => state.settings.languages,
    (state) => state.settings.qualityProfiles,
    createMovieSelector(),
    (
      movieFiles,
      languageProfiles,
      qualityProfiles
    ) => {
      const languages = languageProfiles.items;
      const qualities = getQualities(qualityProfiles.schema.items);

      return {
        items: movieFiles.items,
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

    onDeletePress(movieFileIds) {
      dispatch(deleteMovieFiles({ movieFileIds }));
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

  //
  // Listeners

  onLanguageChange = (movieFileIds, languageId) => {
    const language = _.find(this.props.languages, { id: languageId });
    // TODO - Placeholder till we implement selection of multiple languages
    const languages = [language];
    this.props.dispatchUpdateMovieFiles({ movieFileIds, languages });
  }

  onQualityChange = (movieFileIds, qualityId) => {
    const quality = {
      quality: _.find(this.props.qualities, { id: qualityId }),
      revision: {
        version: 1,
        real: 0
      }
    };

    this.props.dispatchUpdateMovieFiles({ movieFileIds, quality });
  }

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
        onLanguageChange={this.onLanguageChange}
        onQualityChange={this.onQualityChange}
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
