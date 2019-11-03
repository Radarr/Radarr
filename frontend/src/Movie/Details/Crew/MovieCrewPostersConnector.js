import _ from 'lodash';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import MovieCrewPosters from './MovieCrewPosters';

function createMapStateToProps() {
  return createSelector(
    (state) => state.moviePeople.items,
    (people) => {
      const crew = _.reduce(people, (acc, person) => {
        if (person.type === 'crew') {
          acc.push(person);
        }

        return acc;
      }, []);

      return {
        crew
      };
    }
  );
}

export default connect(createMapStateToProps)(MovieCrewPosters);
