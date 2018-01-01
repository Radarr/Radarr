import _ from 'lodash';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { deleteTrackFile } from 'Store/Actions/trackFileActions';
import createAlbumSelector from 'Store/Selectors/createAlbumSelector';
import createDimensionsSelector from 'Store/Selectors/createDimensionsSelector';
import createArtistSelector from 'Store/Selectors/createArtistSelector';
import createCommandsSelector from 'Store/Selectors/createCommandsSelector';
import AlbumSummary from './AlbumSummary';

function createMapStateToProps() {
  return createSelector(
    (state) => state.tracks,
    createAlbumSelector(),
    createCommandsSelector(),
    createDimensionsSelector(),
    createArtistSelector(),
    (tracks, album, commands, dimensions, artist) => {
      const filteredItems = _.filter(tracks.items, { albumId: album.id });
      const mediumSortedItems = _.orderBy(filteredItems, 'absoluteTrackNumber');
      const items = _.orderBy(mediumSortedItems, 'mediumNumber');

      return {
        network: album.label,
        qualityProfileId: artist.qualityProfileId,
        releaseDate: album.releaseDate,
        overview: album.overview,
        items,
        columns: tracks.columns
      };
    }
  );
}

function createMapDispatchToProps(dispatch, props) {
  return {
    onDeleteTrackFile() {
      dispatch(deleteTrackFile({
        id: props.trackFileId,
        albumEntity: props.albumEntity
      }));
    }
  };
}

export default connect(createMapStateToProps, createMapDispatchToProps)(AlbumSummary);
