/* eslint max-params: 0 */
import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import getQualities from 'Utilities/Quality/getQualities';
import createArtistSelector from 'Store/Selectors/createArtistSelector';
import { deleteTrackFiles, updateTrackFiles } from 'Store/Actions/trackFileActions';
import { fetchTracks, clearTracks } from 'Store/Actions/trackActions';
import { fetchQualityProfileSchema } from 'Store/Actions/settingsActions';
import TrackFileEditorTableContent from './TrackFileEditorTableContent';

function createSchemaSelector() {
  return createSelector(
    (state) => state.settings.qualityProfiles,
    (qualityProfiles) => {
      const qualities = getQualities(qualityProfiles.schema.items);

      let error = null;

      if (qualityProfiles.schemaError) {
        error = 'Unable to load qualities';
      }

      return {
        isFetching: qualityProfiles.isSchemaFetching,
        isPopulated: qualityProfiles.isSchemaPopulated,
        error,
        qualities
      };
    }
  );
}

function createMapStateToProps() {
  return createSelector(
    (state, { bookId }) => bookId,
    (state) => state.trackFiles,
    createSchemaSelector(),
    createArtistSelector(),
    (
      bookId,
      trackFiles,
      schema,
      artist
    ) => {
      return {
        ...schema,
        items: trackFiles.items,
        artistType: artist.artistType,
        isDeleting: trackFiles.isDeleting,
        isSaving: trackFiles.isSaving
      };
    }
  );
}

function createMapDispatchToProps(dispatch, props) {
  return {
    dispatchClearTracks() {
      dispatch(clearTracks());
    },

    dispatchFetchTracks(updateProps) {
      dispatch(fetchTracks(updateProps));
    },

    dispatchFetchQualityProfileSchema(name, path) {
      dispatch(fetchQualityProfileSchema());
    },

    dispatchUpdateTrackFiles(updateProps) {
      dispatch(updateTrackFiles(updateProps));
    },

    onDeletePress(trackFileIds) {
      dispatch(deleteTrackFiles({ trackFileIds }));
    }
  };
}

class TrackFileEditorTableContentConnector extends Component {

  //
  // Lifecycle

  componentDidMount() {
    this.props.dispatchFetchQualityProfileSchema();
  }

  //
  // Listeners

  onQualityChange = (trackFileIds, qualityId) => {
    const quality = {
      quality: _.find(this.props.qualities, { id: qualityId }),
      revision: {
        version: 1,
        real: 0
      }
    };

    this.props.dispatchUpdateTrackFiles({ trackFileIds, quality });
  }

  //
  // Render

  render() {
    const {
      dispatchFetchQualityProfileSchema,
      dispatchUpdateTrackFiles,
      dispatchFetchTracks,
      dispatchClearTracks,
      ...otherProps
    } = this.props;

    return (
      <TrackFileEditorTableContent
        {...otherProps}
        onQualityChange={this.onQualityChange}
      />
    );
  }
}

TrackFileEditorTableContentConnector.propTypes = {
  authorId: PropTypes.number.isRequired,
  bookId: PropTypes.number,
  qualities: PropTypes.arrayOf(PropTypes.object).isRequired,
  dispatchFetchTracks: PropTypes.func.isRequired,
  dispatchClearTracks: PropTypes.func.isRequired,
  dispatchFetchQualityProfileSchema: PropTypes.func.isRequired,
  dispatchUpdateTrackFiles: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, createMapDispatchToProps)(TrackFileEditorTableContentConnector);
