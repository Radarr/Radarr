import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import FilterModal from 'Components/Filter/FilterModal';
import { setListMovieFilter } from 'Store/Actions/discoverMovieActions';

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
