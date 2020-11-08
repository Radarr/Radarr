import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { fetchRootFolders } from 'Store/Actions/rootFolderActions';
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

const mapDispatchToProps = {
  fetchRootFolders
};

class MovieCastPostersConnector extends Component {

  //
  // Lifecycle

  componentDidMount() {
    this.props.fetchRootFolders();
  }

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

MovieCastPostersConnector.propTypes = {
  fetchRootFolders: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(MovieCastPostersConnector);
