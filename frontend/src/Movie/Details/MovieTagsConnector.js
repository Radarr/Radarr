import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import createMovieSelector from 'Store/Selectors/createMovieSelector';
import createTagsSelector from 'Store/Selectors/createTagsSelector';
import sortByProp from 'Utilities/Array/sortByProp';
import MovieTags from './MovieTags';

function createMapStateToProps() {
  return createSelector(
    createMovieSelector(),
    createTagsSelector(),
    (movie, tagList) => {
      const tags = movie.tags
        .map((tagId) => tagList.find((tag) => tag.id === tagId))
        .filter((tag) => !!tag)
        .sort(sortByProp('label'))
        .map((tag) => tag.label);

      return {
        tags
      };
    }
  );
}

export default connect(createMapStateToProps)(MovieTags);
