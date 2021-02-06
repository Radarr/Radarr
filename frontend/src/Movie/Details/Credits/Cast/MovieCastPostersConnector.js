import _ from 'lodash';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import MovieCreditPosters from '../MovieCreditPosters';
import MovieCastPoster from './MovieCastPoster';

function createMapStateToProps() {
  return createSelector(
    (state) => state.movieCredits.items,
    (credits) => {
      const cast = _.reduce(credits, (acc, credit) => {
        if (credit.type === 'cast') {
          acc.push(credit);
        }

        return acc;
      }, []);

      return {
        items: cast
      };
    }
  );
}

class MovieCastPostersConnector extends Component {

  //
  // Render

  render() {

    return (
      <MovieCreditPosters
        {...this.props}
        itemComponent={MovieCastPoster}
      />
    );
  }
}

export default connect(createMapStateToProps)(MovieCastPostersConnector);
