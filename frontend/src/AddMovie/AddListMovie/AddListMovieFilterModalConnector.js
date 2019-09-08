import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { setListMovieFilter } from 'Store/Actions/addMovieActions';
import FilterModal from 'Components/Filter/FilterModal';

function createMapStateToProps() {
  return createSelector(
    (state) => state.addMovie.items,
    (state) => state.addMovie.filterBuilderProps,
    (sectionItems, filterBuilderProps) => {
      return {
        sectionItems,
        filterBuilderProps,
        customFilterType: 'addMovie'
      };
    }
  );
}

const mapDispatchToProps = {
  dispatchSetFilter: setListMovieFilter
};

export default connect(createMapStateToProps, mapDispatchToProps)(FilterModal);
