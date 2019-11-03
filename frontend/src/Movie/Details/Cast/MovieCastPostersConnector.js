import _ from 'lodash';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import MovieCastPosters from './MovieCastPosters';

function createMapStateToProps() {
  return createSelector(
    (state) => state.moviePeople.items,
    (people) => {
      const cast = _.reduce(people, (acc, person) => {
        if (person.type === 'cast') {
          acc.push(person);
        }

        return acc;
      }, []);

      return {
        cast
      };
    }
  );
}

export default connect(createMapStateToProps)(MovieCastPosters);
