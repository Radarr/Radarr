import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import createArtistSelector from 'Store/Selectors/createArtistSelector';
import { removeFromBlacklist } from 'Store/Actions/blacklistActions';
import BlacklistRow from './BlacklistRow';

function createMapStateToProps() {
  return createSelector(
    createArtistSelector(),
    (artist) => {
      return {
        artist
      };
    }
  );
}

function createMapDispatchToProps(dispatch, props) {
  return {
    onRemovePress() {
      dispatch(removeFromBlacklist({ id: props.id }));
    }
  };
}

export default connect(createMapStateToProps, createMapDispatchToProps)(BlacklistRow);
