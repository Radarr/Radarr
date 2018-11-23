import _ from 'lodash';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import createMovieSelector from 'Store/Selectors/createMovieSelector';
import createTagsSelector from 'Store/Selectors/createTagsSelector';
import MovieTags from './MovieTags';

function createMapStateToProps() {
  return createSelector(
    createMovieSelector(),
    createTagsSelector(),
    (series, tagList) => {
      const tags = _.reduce(series.tags, (acc, tag) => {
        const matchingTag = _.find(tagList, { id: tag });

        if (matchingTag) {
          acc.push(matchingTag.label);
        }

        return acc;
      }, []);

      return {
        tags
      };
    }
  );
}

export default connect(createMapStateToProps)(MovieTags);
