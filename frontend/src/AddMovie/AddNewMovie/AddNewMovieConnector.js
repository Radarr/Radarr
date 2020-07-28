import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { clearAddMovie, lookupMovie } from 'Store/Actions/addMovieActions';
import { fetchRootFolders } from 'Store/Actions/rootFolderActions';
import { fetchNetImportExclusions } from 'Store/Actions/Settings/netImportExclusions';
import parseUrl from 'Utilities/String/parseUrl';
import AddNewMovie from './AddNewMovie';

function createMapStateToProps() {
  return createSelector(
    (state) => state.addMovie,
    (state) => state.movies.items.length,
    (state) => state.router.location,
    (addMovie, existingMoviesCount, location) => {
      const { params } = parseUrl(location.search);

      return {
        ...addMovie,
        term: params.term,
        hasExistingMovies: existingMoviesCount > 0
      };
    }
  );
}

const mapDispatchToProps = {
  lookupMovie,
  clearAddMovie,
  fetchRootFolders,
  fetchNetImportExclusions
};

class AddNewMovieConnector extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this._movieLookupTimeout = null;
  }

  componentDidMount() {
    this.props.fetchRootFolders();
    this.props.fetchNetImportExclusions();
  }

  componentWillUnmount() {
    if (this._movieLookupTimeout) {
      clearTimeout(this._movieLookupTimeout);
    }

    this.props.clearAddMovie();
  }

  //
  // Listeners

  onMovieLookupChange = (term) => {
    if (this._movieLookupTimeout) {
      clearTimeout(this._movieLookupTimeout);
    }

    if (term.trim() === '') {
      this.props.clearAddMovie();
    } else {
      this._movieLookupTimeout = setTimeout(() => {
        this.props.lookupMovie({ term });
      }, 300);
    }
  }

  onClearMovieLookup = () => {
    this.props.clearAddMovie();
  }

  //
  // Render

  render() {
    const {
      term,
      ...otherProps
    } = this.props;

    return (
      <AddNewMovie
        term={term}
        {...otherProps}
        onMovieLookupChange={this.onMovieLookupChange}
        onClearMovieLookup={this.onClearMovieLookup}
      />
    );
  }
}

AddNewMovieConnector.propTypes = {
  term: PropTypes.string,
  lookupMovie: PropTypes.func.isRequired,
  clearAddMovie: PropTypes.func.isRequired,
  fetchRootFolders: PropTypes.func.isRequired,
  fetchNetImportExclusions: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(AddNewMovieConnector);
