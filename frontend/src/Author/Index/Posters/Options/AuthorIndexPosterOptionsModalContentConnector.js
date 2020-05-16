import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { setAuthorPosterOption } from 'Store/Actions/authorIndexActions';
import AuthorIndexPosterOptionsModalContent from './AuthorIndexPosterOptionsModalContent';

function createMapStateToProps() {
  return createSelector(
    (state) => state.authorIndex,
    (authorIndex) => {
      return authorIndex.posterOptions;
    }
  );
}

function createMapDispatchToProps(dispatch, props) {
  return {
    onChangePosterOption(payload) {
      dispatch(setAuthorPosterOption(payload));
    }
  };
}

export default connect(createMapStateToProps, createMapDispatchToProps)(AuthorIndexPosterOptionsModalContent);
