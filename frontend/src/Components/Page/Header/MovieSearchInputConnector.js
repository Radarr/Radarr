import { connect } from 'react-redux';
import { push } from 'react-router-redux';
import { createSelector } from 'reselect';
import jdu from 'jdu';
import createAllMoviesSelector from 'Store/Selectors/createAllMoviesSelector';
import createTagsSelector from 'Store/Selectors/createTagsSelector';
import MovieSearchInput from './MovieSearchInput';

function createCleanTagsSelector() {
  return createSelector(
    createTagsSelector(),
    (tags) => {
      return tags.map((tag) => {
        const {
          id,
          label
        } = tag;

        return {
          id,
          label,
          cleanLabel: jdu.replace(label).toLowerCase()
        };
      });
    }
  );
}

function createCleanMovieSelector() {
  return createSelector(
    createAllMoviesSelector(),
    createCleanTagsSelector(),
    (allMovies, allTags) => {
      return allMovies.map((movie) => {
        const {
          title,
          titleSlug,
          sortTitle,
          images,
          alternateTitles = [],
          tags = []
        } = movie;

        return {
          title,
          titleSlug,
          sortTitle,
          images,
          cleanTitle: jdu.replace(title).toLowerCase(),
          alternateTitles: alternateTitles.map((alternateTitle) => {
            return {
              title: alternateTitle.title,
              sortTitle: alternateTitle.sortTitle,
              cleanTitle: jdu.replace(alternateTitle.title).toLowerCase()
            };
          }),
          tags: tags.map((id) => {
            return allTags.find((tag) => tag.id === id);
          })
        };
      }).sort((a, b) => {
        if (a.sortTitle < b.sortTitle) {
          return -1;
        }
        if (a.sortTitle > b.sortTitle) {
          return 1;
        }

        return 0;
      });
    }
  );
}

function createMapStateToProps() {
  return createSelector(
    createCleanMovieSelector(),
    (movie) => {
      return {
        movie
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
