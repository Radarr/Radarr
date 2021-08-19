import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { removeBlocklistItem } from 'Store/Actions/blocklistActions';
import createMovieSelector from 'Store/Selectors/createMovieSelector';
import BlocklistRow from './BlocklistRow';

function createMapStateToProps() {
  return createSelector(
    createMovieSelector(),
    (movie) => {
      return {
        movie
      };
    }
  );
}

function createMapDispatchToProps(dispatch, props) {
  return {
    onRemovePress() {
      dispatch(removeBlocklistItem({ id: props.id }));
    }
  };
}

export default connect(createMapStateToProps, createMapDispatchToProps)(BlocklistRow);
