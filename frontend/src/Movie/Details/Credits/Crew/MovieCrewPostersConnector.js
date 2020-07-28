import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { fetchRootFolders } from 'Store/Actions/rootFolderActions';
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
        items: crew
      };
    }
  );
}

const mapDispatchToProps = {
  fetchRootFolders
};

class MovieCrewPostersConnector extends Component {

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
        itemComponent={MovieCrewPoster}
      />
    );
  }
}

MovieCrewPostersConnector.propTypes = {
  fetchRootFolders: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(MovieCrewPostersConnector);
