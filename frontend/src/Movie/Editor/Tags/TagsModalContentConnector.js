import _ from 'lodash';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import createAllMoviesSelector from 'Store/Selectors/createAllMoviesSelector';
import createTagsSelector from 'Store/Selectors/createTagsSelector';
import TagsModalContent from './TagsModalContent';

function createMapStateToProps() {
  return createSelector(
    (state, { movieIds }) => movieIds,
    createAllMoviesSelector(),
    createTagsSelector(),
    (movieIds, allMovies, tagList) => {
      const movies = _.intersectionWith(allMovies, movieIds, (s, id) => {
        return s.id === id;
      });

      const movieTags = _.uniq(_.concat(..._.map(movies, 'tags')));

      return {
        movieTags,
        tagList
      };
    }
  );
}

function createMapDispatchToProps(dispatch, props) {
  return {
    onAction() {
      // Do something
    }
  };
}

export default connect(createMapStateToProps, createMapDispatchToProps)(TagsModalContent);
