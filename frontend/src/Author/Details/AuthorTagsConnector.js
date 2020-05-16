import _ from 'lodash';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import createAuthorSelector from 'Store/Selectors/createAuthorSelector';
import createTagsSelector from 'Store/Selectors/createTagsSelector';
import AuthorTags from './AuthorTags';

function createMapStateToProps() {
  return createSelector(
    createAuthorSelector(),
    createTagsSelector(),
    (author, tagList) => {
      const tags = _.reduce(author.tags, (acc, tag) => {
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

export default connect(createMapStateToProps)(AuthorTags);
