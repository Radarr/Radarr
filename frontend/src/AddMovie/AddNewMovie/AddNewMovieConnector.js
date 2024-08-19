import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { clearAddMovie, lookupMovie } from 'Store/Actions/addMovieActions';
import { clearMovieFiles, fetchMovieFiles } from 'Store/Actions/movieFileActions';
import { clearQueueDetails, fetchQueueDetails } from 'Store/Actions/queueActions';
import { fetchRootFolders } from 'Store/Actions/rootFolderActions';
import createUISettingsSelector from 'Store/Selectors/createUISettingsSelector';
import hasDifferentItems from 'Utilities/Object/hasDifferentItems';
import selectUniqueIds from 'Utilities/Object/selectUniqueIds';
import parseUrl from 'Utilities/String/parseUrl';
import AddNewMovie from './AddNewMovie';

function createMapStateToProps() {
  return createSelector(
    (state) => state.addMovie,
    (state) => state.movies.items.length,
    (state) => state.router.location,
    createUISettingsSelector(),
    (addMovie, existingMoviesCount, location, uiSettings) => {
      const { params } = parseUrl(location.search);

      return {
        ...addMovie,
        term: params.term,
        hasExistingMovies: existingMoviesCount > 0,
        colorImpairedMode: uiSettings.enableColorImpairedMode
      };
    }
  );
}

const mapDispatchToProps = {
  lookupMovie,
  clearAddMovie,
  fetchRootFolders,
  fetchQueueDetails,
  clearQueueDetails,
  fetchMovieFiles,
  clearMovieFiles
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
    this.props.fetchQueueDetails();
  }

  componentDidUpdate(prevProps) {
    const {
      items
    } = this.props;

    if (hasDifferentItems(prevProps.items, items)) {
      const movieIds = selectUniqueIds(items, 'internalId');

      if (movieIds.length) {
        this.props.fetchMovieFiles({ movieId: movieIds });
      }
    }
  }

  componentWillUnmount() {
    if (this._movieLookupTimeout) {
      clearTimeout(this._movieLookupTimeout);
    }

    this.props.clearAddMovie();
    this.props.clearQueueDetails();
    this.props.clearMovieFiles();
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
  };

  onClearMovieLookup = () => {
    this.props.clearAddMovie();
  };

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
  items: PropTypes.arrayOf(PropTypes.object).isRequired,
  lookupMovie: PropTypes.func.isRequired,
  clearAddMovie: PropTypes.func.isRequired,
  fetchRootFolders: PropTypes.func.isRequired,
  fetchQueueDetails: PropTypes.func.isRequired,
  clearQueueDetails: PropTypes.func.isRequired,
  fetchMovieFiles: PropTypes.func.isRequired,
  clearMovieFiles: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(AddNewMovieConnector);
