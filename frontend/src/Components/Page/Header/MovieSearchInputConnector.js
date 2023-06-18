import { push } from 'connected-react-router';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import createAllMoviesSelector from 'Store/Selectors/createAllMoviesSelector';
import createDeepEqualSelector from 'Store/Selectors/createDeepEqualSelector';
import createTagsSelector from 'Store/Selectors/createTagsSelector';
import MovieSearchInput from './MovieSearchInput';

function createCleanMovieSelector() {
  return createSelector(
    createAllMoviesSelector(),
    createTagsSelector(),
    (allMovies, allTags) => {
      return allMovies.map((movie) => {
        const {
          title,
          titleSlug,
          sortTitle,
          year,
          images,
          alternateTitles = [],
          tmdbId,
          imdbId,
          tags = []
        } = movie;

        return {
          title,
          titleSlug,
          sortTitle,
          year,
          images,
          alternateTitles,
          tmdbId,
          imdbId,
          firstCharacter: title.charAt(0).toLowerCase(),
          tags: tags.reduce((acc, id) => {
            const matchingTag = allTags.find((tag) => tag.id === id);

            if (matchingTag) {
              acc.push(matchingTag);
            }

            return acc;
          }, [])
        };
      });
    }
  );
}

function createMapStateToProps() {
  return createDeepEqualSelector(
    createCleanMovieSelector(),
    (movies) => {
      return {
        movies
      };
    }
  );
}

function createMapDispatchToProps(dispatch, props) {
  return {
    onGoToMovie(titleSlug) {
      dispatch(push(`${window.Radarr.urlBase}/movie/${titleSlug}`));
    },

    onGoToAddNewMovie(query) {
      dispatch(push(`${window.Radarr.urlBase}/add/new?term=${encodeURIComponent(query)}`));
    }
  };
}

export default connect(createMapStateToProps, createMapDispatchToProps)(MovieSearchInput);
