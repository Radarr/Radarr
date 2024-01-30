import _ from 'lodash';
import { update } from 'Store/Actions/baseActions';

function updateMovies(section, movies, movieIds, options) {
  const data = _.reduce(movies, (result, item) => {
    if (movieIds.indexOf(item.id) > -1) {
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

export default updateMovies;
