import _ from 'lodash';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import createArtistSelector from 'Store/Selectors/createArtistSelector';
import createTrackFileSelector from 'Store/Selectors/createTrackFileSelector';
import createCommandsSelector from 'Store/Selectors/createCommandsSelector';
import AlbumRow from './AlbumRow';

function createMapStateToProps() {
  return createSelector(
    (state, { id }) => id,
    (state, { sceneSeasonNumber }) => sceneSeasonNumber,
    createArtistSelector(),
    createTrackFileSelector(),
    createCommandsSelector(),
    (id, sceneSeasonNumber, series, trackFile, commands) => {
      const alternateTitles = sceneSeasonNumber ? _.filter(series.alternateTitles, { sceneSeasonNumber }) : [];

      return {
        artistMonitored: series.monitored,
        seriesType: series.seriesType,
        trackFilePath: trackFile ? trackFile.path : null,
        trackFileRelativePath: trackFile ? trackFile.relativePath : null,
        alternateTitles
      };
    }
  );
}
export default connect(createMapStateToProps)(AlbumRow);
