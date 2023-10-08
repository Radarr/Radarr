import _ from 'lodash';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import MovieCreditPosters from '../MovieCreditPosters';
import MovieCrewPoster from './MovieCrewPoster';

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

      return {
        items: _.uniqBy(crew, 'personName')
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
