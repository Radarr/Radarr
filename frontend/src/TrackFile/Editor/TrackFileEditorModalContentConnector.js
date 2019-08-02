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
import TrackFileEditorModalContent from './TrackFileEditorModalContent';

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
    (state, { albumId }) => albumId,
    (state) => state.tracks,
    (state) => state.trackFiles,
    createSchemaSelector(),
    createArtistSelector(),
    (
      albumId,
      tracks,
      trackFiles,
      schema,
      artist
    ) => {
      const filtered = _.filter(tracks.items, (track) => {
        if (albumId >= 0 && track.albumId !== albumId) {
          return false;
        }

        if (!track.trackFileId) {
          return false;
        }

        return _.some(trackFiles.items, { id: track.trackFileId });
      });

      const sorted = _.orderBy(filtered, ['albumId', 'absoluteTrackNumber'], ['desc', 'asc']);

      const items = _.map(sorted, (track) => {
        const trackFile = _.find(trackFiles.items, { id: track.trackFileId });

        return {
          relativePath: trackFile.relativePath,
          quality: trackFile.quality,
          ...track
        };
      });

      return {
        ...schema,
        items,
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

class TrackFileEditorModalContentConnector extends Component {

  //
  // Lifecycle

  componentDidMount() {
    const artistId = this.props.artistId;
    const albumId = this.props.albumId;

    this.props.dispatchFetchTracks({ artistId, albumId });

    this.props.dispatchFetchQualityProfileSchema();
  }

  componentWillUnmount() {
    this.props.dispatchClearTracks();
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
      <TrackFileEditorModalContent
        {...otherProps}
        onQualityChange={this.onQualityChange}
      />
    );
  }
}

TrackFileEditorModalContentConnector.propTypes = {
  artistId: PropTypes.number.isRequired,
  albumId: PropTypes.number,
  qualities: PropTypes.arrayOf(PropTypes.object).isRequired,
  dispatchFetchTracks: PropTypes.func.isRequired,
  dispatchClearTracks: PropTypes.func.isRequired,
  dispatchFetchQualityProfileSchema: PropTypes.func.isRequired,
  dispatchUpdateTrackFiles: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, createMapDispatchToProps)(TrackFileEditorModalContentConnector);
