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
import { fetchLanguageProfileSchema, fetchQualityProfileSchema } from 'Store/Actions/settingsActions';
import TrackFileEditorModalContent from './TrackFileEditorModalContent';

function createMapStateToProps() {
  return createSelector(
    (state, { albumId }) => albumId,
    (state) => state.tracks,
    (state) => state.trackFiles,
    (state) => state.settings.languageProfiles.schema,
    (state) => state.settings.qualityProfiles.schema,
    createArtistSelector(),
    (
      albumId,
      tracks,
      trackFiles,
      languageProfilesSchema,
      qualityProfileSchema,
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
          language: trackFile.language,
          quality: trackFile.quality,
          ...track
        };
      });

      const languages = _.map(languageProfilesSchema.languages, 'language');
      const qualities = getQualities(qualityProfileSchema.items);

      return {
        items,
        artistType: artist.artistType,
        isDeleting: trackFiles.isDeleting,
        isSaving: trackFiles.isSaving,
        languages,
        qualities
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

    dispatchFetchLanguageProfileSchema(name, path) {
      dispatch(fetchLanguageProfileSchema());
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

    this.props.dispatchFetchLanguageProfileSchema();
    this.props.dispatchFetchQualityProfileSchema();
  }

  componentWillUnmount() {
    this.props.dispatchClearTracks();
  }

  //
  // Render

  //
  // Listeners

  onLanguageChange = (trackFileIds, languageId) => {
    const language = _.find(this.props.languages, { id: languageId });

    this.props.dispatchUpdateTrackFiles({ trackFileIds, language });
  }

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

  render() {
    const {
      dispatchFetchLanguageProfileSchema,
      dispatchFetchQualityProfileSchema,
      dispatchUpdateTrackFiles,
      dispatchFetchTracks,
      dispatchClearTracks,
      ...otherProps
    } = this.props;

    return (
      <TrackFileEditorModalContent
        {...otherProps}
        onLanguageChange={this.onLanguageChange}
        onQualityChange={this.onQualityChange}
      />
    );
  }
}

TrackFileEditorModalContentConnector.propTypes = {
  artistId: PropTypes.number.isRequired,
  albumId: PropTypes.number,
  languages: PropTypes.arrayOf(PropTypes.object).isRequired,
  qualities: PropTypes.arrayOf(PropTypes.object).isRequired,
  dispatchFetchTracks: PropTypes.func.isRequired,
  dispatchClearTracks: PropTypes.func.isRequired,
  dispatchFetchLanguageProfileSchema: PropTypes.func.isRequired,
  dispatchFetchQualityProfileSchema: PropTypes.func.isRequired,
  dispatchUpdateTrackFiles: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, createMapDispatchToProps)(TrackFileEditorModalContentConnector);
