/* eslint max-params: 0 */
import _ from 'lodash';
import { update } from 'Store/Actions/baseActions';

function updateAlbums(dispatch, section, episodes, albumIds, options) {
  const data = _.reduce(episodes, (result, item) => {
    if (albumIds.indexOf(item.id) > -1) {
      result.push({
        ...item,
        ...options
      });
    } else {
      result.push(item);
    }

    return result;
  }, []);

  dispatch(update({ section, data }));
}

export default updateAlbums;
