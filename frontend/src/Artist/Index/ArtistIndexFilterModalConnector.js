import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import * as artistIndexActions from 'Store/Actions/artistIndexActions';
import FilterModal from 'Components/Filter/FilterModal';

function createMapStateToProps() {
  return createSelector(
    (state) => state.artist.items,
    (state) => state.artistIndex.filterBuilderProps,
    (sectionItems, filterBuilderProps) => {
      return {
        sectionItems,
        filterBuilderProps
      };
    }
  );
}

function createMapDispatchToProps(dispatch, props) {
  return {
    onRemoveCustomFilterPress(payload) {
      dispatch(artistIndexActions.removeArtistCustomFilter(payload));
    },

    onSaveCustomFilterPress(payload) {
      dispatch(artistIndexActions.saveArtistCustomFilter(payload));
    }
  };
}

export default connect(createMapStateToProps, createMapDispatchToProps)(FilterModal);
