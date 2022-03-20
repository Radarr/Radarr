import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import * as commandNames from 'Commands/commandNames';
import { executeCommand } from 'Store/Actions/commandActions';
import createAllMoviesSelector from 'Store/Selectors/createAllMoviesSelector';
import OrganizeMovieModalContent from './OrganizeMovieModalContent';

function createMapStateToProps() {
  return createSelector(
    (state, { movieIds }) => movieIds,
    createAllMoviesSelector(),
    (movieIds, allMovies) => {
      const movies = _.intersectionWith(allMovies, movieIds, (s, id) => {
        return s.id === id;
      });

      const sortedMovies = _.orderBy(movies, 'sortTitle');
      const movieTitles = _.map(sortedMovies, 'title');

      return {
        movieTitles
      };
    }
  );
}

const mapDispatchToProps = {
  executeCommand
};

class OrganizeMovieModalContentConnector extends Component {

  //
  // Listeners

  onOrganizeMoviePress = () => {
    this.props.executeCommand({
      name: commandNames.RENAME_MOVIE,
      movieIds: this.props.movieIds
    });

    this.props.onModalClose(true);
  };

  //
  // Render

  render(props) {
    return (
      <OrganizeMovieModalContent
        {...this.props}
        onOrganizeMoviePress={this.onOrganizeMoviePress}
      />
    );
  }
}

OrganizeMovieModalContentConnector.propTypes = {
  movieIds: PropTypes.arrayOf(PropTypes.number).isRequired,
  onModalClose: PropTypes.func.isRequired,
  executeCommand: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(OrganizeMovieModalContentConnector);
