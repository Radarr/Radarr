import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import FilterModal from 'Components/Filter/FilterModal';
import { setMovieFilter } from 'Store/Actions/movieIndexActions';

function createMapStateToProps() {
  return createSelector(
    (state) => state.movies.items,
    (state) => state.movieIndex.filterBuilderProps,
    (sectionItems, filterBuilderProps) => {
      return {
        sectionItems,
        filterBuilderProps,
        customFilterType: 'movieIndex'
      };
    }
  );
}

const mapDispatchToProps = {
  dispatchSetFilter: setMovieFilter
};

export default connect(createMapStateToProps, mapDispatchToProps)(FilterModal);
