import _ from 'lodash';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import createArtistSelector from 'Store/Selectors/createArtistSelector';
import createEpisodeFileSelector from 'Store/Selectors/createEpisodeFileSelector';
import createCommandsSelector from 'Store/Selectors/createCommandsSelector';
import AlbumRow from './AlbumRow';

function createMapStateToProps() {
  return createSelector(
    (state, { id }) => id,
    (state, { sceneSeasonNumber }) => sceneSeasonNumber,
    createArtistSelector(),
    createEpisodeFileSelector(),
    createCommandsSelector(),
    (id, sceneSeasonNumber, series, episodeFile, commands) => {
      const alternateTitles = sceneSeasonNumber ? _.filter(series.alternateTitles, { sceneSeasonNumber }) : [];

      return {
        artistMonitored: series.monitored,
        seriesType: series.seriesType,
        episodeFilePath: episodeFile ? episodeFile.path : null,
        episodeFileRelativePath: episodeFile ? episodeFile.relativePath : null,
        alternateTitles
      };
    }
  );
}
export default connect(createMapStateToProps)(AlbumRow);
