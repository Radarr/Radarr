import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { deleteMovie } from 'Store/Actions/movieActions';
import createMovieSelector from 'Store/Selectors/createMovieSelector';
import DeleteMovieModalContent from './DeleteMovieModalContent';

function createMapStateToProps() {
  return createSelector(
    createMovieSelector(),
    (movie) => {
      return movie;
    }
  );
}

const mapDispatchToProps = {
  deleteMovie
};

class DeleteMovieModalContentConnector extends Component {

  //
  // Listeners

  onDeletePress = (deleteFiles, addNetImportExclusion) => {
    this.props.deleteMovie({
      id: this.props.movieId,
      deleteFiles,
      addNetImportExclusion
    });

    this.props.onModalClose(true);
  }

  //
  // Render

  render() {
    return (
      <DeleteMovieModalContent
        {...this.props}
        onDeletePress={this.onDeletePress}
      />
    );
  }
}

DeleteMovieModalContentConnector.propTypes = {
  movieId: PropTypes.number.isRequired,
  onModalClose: PropTypes.func.isRequired,
  deleteMovie: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(DeleteMovieModalContentConnector);
