import _ from 'lodash';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import MovieCreditPosters from '../MovieCreditPosters';
import MovieCrewPoster from './MovieCrewPoster';

function crewSort(a, b) {
  const jobOrder = ['Director', 'Writer', 'Producer', 'Executive Producer', 'Director of Photography'];

  const indexA = jobOrder.indexOf(a.job);
  const indexB = jobOrder.indexOf(b.job);

  if (indexA === -1 && indexB === -1) {
    return 0;
  } else if (indexA === -1) {
    return 1;
  } else if (indexB === -1) {
    return -1;
  }

  if (indexA < indexB) {
    return -1;
  } else if (indexA > indexB) {
    return 1;
  }

  return 0;
}

function createMapStateToProps() {
  return createSelector(
    (state) => state.movieCredits.items,
    (credits) => {
      const crew = _.reduce(credits, (acc, credit) => {
        if (credit.type === 'crew') {
          acc.push(credit);
        }

        return acc;
      }, []);

      const sortedCrew = crew.sort(crewSort);

      return {
        items: _.uniqBy(sortedCrew, 'personName')
      };
    }
  );
}

class MovieCrewPostersConnector extends Component {

  //
  // Render

  render() {

    return (
      <MovieCreditPosters
        {...this.props}
        itemComponent={MovieCrewPoster}
      />
    );
  }
}

export default connect(createMapStateToProps)(MovieCrewPostersConnector);
