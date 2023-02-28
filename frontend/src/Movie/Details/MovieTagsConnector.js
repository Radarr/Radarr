import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import createMovieSelector from 'Store/Selectors/createMovieSelector';
import createTagsSelector from 'Store/Selectors/createTagsSelector';
import MovieTags from './MovieTags';

function createMapStateToProps() {
  return createSelector(
    createMovieSelector(),
    createTagsSelector(),
    (movie, tagList) => {
      const tags = movie.tags
        .map((tagId) => tagList.find((tag) => tag.id === tagId))
        .filter((tag) => !!tag)
        .map((tag) => tag.label)
        .sort((a, b) => a.localeCompare(b));

      return {
        tags
      };
    }
  );
}

export default connect(createMapStateToProps)(MovieTags);
