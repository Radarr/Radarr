import { connect } from 'react-redux';
import { setAuthorTableOption } from 'Store/Actions/authorIndexActions';
import AuthorIndexHeader from './AuthorIndexHeader';

function createMapDispatchToProps(dispatch, props) {
  return {
    onTableOptionChange(payload) {
      dispatch(setAuthorTableOption(payload));
    }
  };
}

export default connect(undefined, createMapDispatchToProps)(AuthorIndexHeader);
