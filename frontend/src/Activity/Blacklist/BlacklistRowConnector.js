import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { removeBlacklistItem } from 'Store/Actions/blacklistActions';
import createMovieSelector from 'Store/Selectors/createMovieSelector';
import BlacklistRow from './BlacklistRow';

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
      dispatch(removeBlacklistItem({ id: props.id }));
    }
  };
}

export default connect(createMapStateToProps, createMapDispatchToProps)(BlacklistRow);
