import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { setAlbumStudioFilter } from 'Store/Actions/albumStudioActions';
import FilterModal from 'Components/Filter/FilterModal';

function createMapStateToProps() {
  return createSelector(
    (state) => state.artist.items,
    (state) => state.albumStudio.filterBuilderProps,
    (sectionItems, filterBuilderProps) => {
      return {
        sectionItems,
        filterBuilderProps,
        customFilterType: 'albumStudio'
      };
    }
  );
}

const mapDispatchToProps = {
  dispatchSetFilter: setAlbumStudioFilter
};

export default connect(createMapStateToProps, mapDispatchToProps)(FilterModal);
