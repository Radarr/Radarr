import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { setListMovieFilter } from 'Store/Actions/discoverMovieActions';
import FilterModal from 'Components/Filter/FilterModal';

function createMapStateToProps() {
  return createSelector(
    (state) => state.discoverMovie.items,
    (state) => state.discoverMovie.filterBuilderProps,
    (sectionItems, filterBuilderProps) => {
      return {
        sectionItems,
        filterBuilderProps,
        customFilterType: 'discoverMovie'
      };
    }
  );
}

const mapDispatchToProps = {
  dispatchSetFilter: setListMovieFilter
};

export default connect(createMapStateToProps, mapDispatchToProps)(FilterModal);
