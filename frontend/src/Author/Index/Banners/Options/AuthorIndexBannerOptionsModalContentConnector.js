import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { setAuthorBannerOption } from 'Store/Actions/authorIndexActions';
import AuthorIndexBannerOptionsModalContent from './AuthorIndexBannerOptionsModalContent';

function createMapStateToProps() {
  return createSelector(
    (state) => state.authorIndex,
    (authorIndex) => {
      return authorIndex.bannerOptions;
    }
  );
}

function createMapDispatchToProps(dispatch, props) {
  return {
    onChangeBannerOption(payload) {
      dispatch(setAuthorBannerOption(payload));
    }
  };
}

export default connect(createMapStateToProps, createMapDispatchToProps)(AuthorIndexBannerOptionsModalContent);
