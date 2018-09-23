import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { setArtistEditorFilter } from 'Store/Actions/artistEditorActions';
import FilterModal from 'Components/Filter/FilterModal';

function createMapStateToProps() {
  return createSelector(
    (state) => state.artist.items,
    (state) => state.artistEditor.filterBuilderProps,
    (sectionItems, filterBuilderProps) => {
      return {
        sectionItems,
        filterBuilderProps,
        customFilterType: 'artistEditor'
      };
    }
  );
}

const mapDispatchToProps = {
  dispatchSetFilter: setArtistEditorFilter
};

export default connect(createMapStateToProps, mapDispatchToProps)(FilterModal);
