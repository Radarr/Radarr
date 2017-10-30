import _ from 'lodash';
import { createSelector } from 'reselect';
import episodeEntities from 'Album/episodeEntities';

function createEpisodeSelector() {
  return createSelector(
    (state, { albumId }) => albumId,
    (state, { episodeEntity = episodeEntities.EPISODES }) => _.get(state, episodeEntity, { items: [] }),
    (albumId, episodes) => {
      return _.find(episodes.items, { id: albumId });
    }
  );
}

export default createEpisodeSelector;
