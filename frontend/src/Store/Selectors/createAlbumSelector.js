import _ from 'lodash';
import { createSelector } from 'reselect';
import albumEntities from 'Album/albumEntities';

function createAlbumSelector() {
  return createSelector(
    (state, { bookId }) => bookId,
    (state, { albumEntity = albumEntities.ALBUMS }) => _.get(state, albumEntity, { items: [] }),
    (bookId, albums) => {
      return _.find(albums.items, { id: bookId });
    }
  );
}

export default createAlbumSelector;
