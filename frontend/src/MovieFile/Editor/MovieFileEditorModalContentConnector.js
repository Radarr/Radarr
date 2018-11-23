/* eslint max-params: 0 */
import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import getQualities from 'Utilities/Quality/getQualities';
import createMovieSelector from 'Store/Selectors/createMovieSelector';
import { deleteMovieFiles, updateMovieFiles } from 'Store/Actions/movieFileActions';
import { fetchQualityProfileSchema } from 'Store/Actions/settingsActions';
import MovieFileEditorModalContent from './MovieFileEditorModalContent';

function createMapStateToProps() {
  return createSelector(
    (state) => state.movieFiles,
    (state) => state.settings.qualityProfiles.schema,
    createMovieSelector(),
    (
      movieFiles,
      qualityProfileSchema,
      movie
    ) => {
      const qualities = getQualities(qualityProfileSchema.items);

      return {
        items: movieFiles.items,
        isDeleting: movieFiles.isDeleting,
        isSaving: movieFiles.isSaving,
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

    dispatchUpdateMovieFiles(updateProps) {
      dispatch(updateMovieFiles(updateProps));
    },

    onDeletePress(episodeFileIds) {
      dispatch(deleteMovieFiles({ episodeFileIds }));
    }
  };
}

class MovieFileEditorModalContentConnector extends Component {

  //
  // Lifecycle

  componentDidMount() {
    this.props.dispatchFetchQualityProfileSchema();
  }

  //
  // Render

  //
  // Listeners

  onQualityChange = (episodeFileIds, qualityId) => {
    const quality = {
      quality: _.find(this.props.qualities, { id: qualityId }),
      revision: {
        version: 1,
        real: 0
      }
    };

    this.props.dispatchUpdateMovieFiles({ episodeFileIds, quality });
  }

  render() {
    const {
      dispatchFetchQualityProfileSchema,
      dispatchUpdateMovieFiles,
      ...otherProps
    } = this.props;

    return (
      <MovieFileEditorModalContent
        {...otherProps}
        onQualityChange={this.onQualityChange}
      />
    );
  }
}

MovieFileEditorModalContentConnector.propTypes = {
  movieId: PropTypes.number.isRequired,
  languages: PropTypes.arrayOf(PropTypes.object).isRequired,
  qualities: PropTypes.arrayOf(PropTypes.object).isRequired,
  dispatchFetchQualityProfileSchema: PropTypes.func.isRequired,
  dispatchUpdateMovieFiles: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, createMapDispatchToProps)(MovieFileEditorModalContentConnector);
