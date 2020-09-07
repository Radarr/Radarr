import { push } from 'connected-react-router';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import createAllAuthorSelector from 'Store/Selectors/createAllAuthorsSelector';
import createDeepEqualSelector from 'Store/Selectors/createDeepEqualSelector';
import createTagsSelector from 'Store/Selectors/createTagsSelector';
import AuthorSearchInput from './AuthorSearchInput';

function createCleanAuthorSelector() {
  return createSelector(
    createAllAuthorSelector(),
    createTagsSelector(),
    (allAuthors, allTags) => {
      return allAuthors.map((author) => {
        const {
          authorName,
          sortName,
          images,
          titleSlug,
          tags = []
        } = author;

        return {
          authorName,
          sortName,
          titleSlug,
          images,
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
    createCleanAuthorSelector(),
    (authors) => {
      return {
        authors
      };
    }
  );
}

function createMapDispatchToProps(dispatch, props) {
  return {
    onGoToAuthor(titleSlug) {
      dispatch(push(`${window.Readarr.urlBase}/author/${titleSlug}`));
    },

    onGoToAddNewAuthor(query) {
      dispatch(push(`${window.Readarr.urlBase}/add/search?term=${encodeURIComponent(query)}`));
    }
  };
}

export default connect(createMapStateToProps, createMapDispatchToProps)(AuthorSearchInput);
