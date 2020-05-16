import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { setAuthorOverviewOption } from 'Store/Actions/authorIndexActions';
import AuthorIndexOverviewOptionsModalContent from './AuthorIndexOverviewOptionsModalContent';

function createMapStateToProps() {
  return createSelector(
    (state) => state.authorIndex,
    (authorIndex) => {
      return authorIndex.overviewOptions;
    }
  );
}

function createMapDispatchToProps(dispatch, props) {
  return {
    onChangeOverviewOption(payload) {
      dispatch(setAuthorOverviewOption(payload));
    }
  };
}

export default connect(createMapStateToProps, createMapDispatchToProps)(AuthorIndexOverviewOptionsModalContent);
