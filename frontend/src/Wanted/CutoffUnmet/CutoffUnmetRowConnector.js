import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import createArtistSelector from 'Store/Selectors/createArtistSelector';
import CutoffUnmetRow from './CutoffUnmetRow';

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

export default connect(createMapStateToProps)(CutoffUnmetRow);
