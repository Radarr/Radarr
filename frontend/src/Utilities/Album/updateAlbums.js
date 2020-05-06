import _ from 'lodash';
import { update } from 'Store/Actions/baseActions';

function updateAlbums(section, albums, bookIds, options) {
  const data = _.reduce(albums, (result, item) => {
    if (bookIds.indexOf(item.id) > -1) {
      result.push({
        ...item,
        ...options
      });
    } else {
      result.push(item);
    }

    return result;
  }, []);

  return update({ section, data });
}

export default updateAlbums;
