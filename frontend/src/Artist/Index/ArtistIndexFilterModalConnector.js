import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { setArtistFilter } from 'Store/Actions/artistIndexActions';
import FilterModal from 'Components/Filter/FilterModal';

function createMapStateToProps() {
  return createSelector(
    (state) => state.artist.items,
    (state) => state.artistIndex.filterBuilderProps,
    (sectionItems, filterBuilderProps) => {
      return {
        sectionItems,
        filterBuilderProps,
        customFilterType: 'artistIndex'
      };
    }
  );
}

const mapDispatchToProps = {
  dispatchSetFilter: setArtistFilter
};

export default connect(createMapStateToProps, mapDispatchToProps)(FilterModal);
