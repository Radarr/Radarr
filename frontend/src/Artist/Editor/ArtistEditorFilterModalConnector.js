import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import * as artistEditorActions from 'Store/Actions/artistEditorActions';
import FilterModal from 'Components/Filter/FilterModal';

function createMapStateToProps() {
  return createSelector(
    (state) => state.artist.items,
    (state) => state.artistEditor.filterBuilderProps,
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
      dispatch(artistEditorActions.removeArtistEditorCustomFilter(payload));
    },

    onSaveCustomFilterPress(payload) {
      dispatch(artistEditorActions.saveArtistEditorCustomFilter(payload));
    }
  };
}

export default connect(createMapStateToProps, createMapDispatchToProps)(FilterModal);
