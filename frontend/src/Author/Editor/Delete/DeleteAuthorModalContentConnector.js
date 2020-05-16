import _ from 'lodash';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import createAllAuthorSelector from 'Store/Selectors/createAllAuthorsSelector';
import { bulkDeleteAuthor } from 'Store/Actions/authorEditorActions';
import DeleteAuthorModalContent from './DeleteAuthorModalContent';

function createMapStateToProps() {
  return createSelector(
    (state, { authorIds }) => authorIds,
    createAllAuthorSelector(),
    (authorIds, allAuthors) => {
      const selectedAuthor = _.intersectionWith(allAuthors, authorIds, (s, id) => {
        return s.id === id;
      });

      const sortedAuthor = _.orderBy(selectedAuthor, 'sortName');
      const author = _.map(sortedAuthor, (s) => {
        return {
          authorName: s.authorName,
          path: s.path
        };
      });

      return {
        author
      };
    }
  );
}

function createMapDispatchToProps(dispatch, props) {
  return {
    onDeleteSelectedPress(deleteFiles) {
      dispatch(bulkDeleteAuthor({
        authorIds: props.authorIds,
        deleteFiles
      }));

      props.onModalClose();
    }
  };
}

export default connect(createMapStateToProps, createMapDispatchToProps)(DeleteAuthorModalContent);
