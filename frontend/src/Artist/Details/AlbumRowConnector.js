/* eslint max-params: 0 */
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import createArtistSelector from 'Store/Selectors/createArtistSelector';
import createTrackFileSelector from 'Store/Selectors/createTrackFileSelector';
import AlbumRow from './AlbumRow';

function createMapStateToProps() {
  return createSelector(
    createArtistSelector(),
    createTrackFileSelector(),
    (artist = {}, trackFile) => {
      return {
        artistMonitored: artist.monitored,
        trackFilePath: trackFile ? trackFile.path : null
      };
    }
  );
}
export default connect(createMapStateToProps)(AlbumRow);
