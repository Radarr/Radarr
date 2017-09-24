import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { deleteEpisodeFile } from 'Store/Actions/episodeFileActions';
import createEpisodeSelector from 'Store/Selectors/createEpisodeSelector';
import createTrackSelector from 'Store/Selectors/createTrackSelector';
import createEpisodeFileSelector from 'Store/Selectors/createEpisodeFileSelector';
import createDimensionsSelector from 'Store/Selectors/createDimensionsSelector';
import createArtistSelector from 'Store/Selectors/createArtistSelector';
import createCommandsSelector from 'Store/Selectors/createCommandsSelector';
import EpisodeSummary from './EpisodeSummary';

function createMapStateToProps() {
  return createSelector(
    (state, { episode }) => episode,
    (state) => state.tracks,
    createEpisodeSelector(),
    createCommandsSelector(),
    createDimensionsSelector(),
    (albumId, tracks, episode, commands, dimensions) => {
      return {
        network: episode.label,
        qualityProfileId: episode.profileId,
        airDateUtc: episode.releaseDate,
        overview: episode.overview,
        items: tracks.items,
        columns: tracks.columns
      };
    }
  );
}

function createMapDispatchToProps(dispatch, props) {
  return {
    onDeleteEpisodeFile() {
      dispatch(deleteEpisodeFile({
        id: props.episodeFileId,
        episodeEntity: props.episodeEntity
      }));
    }
  };
}

export default connect(createMapStateToProps, createMapDispatchToProps)(EpisodeSummary);
