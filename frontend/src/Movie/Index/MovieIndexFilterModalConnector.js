import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { setMovieFilter } from 'Store/Actions/movieIndexActions';
import FilterModal from 'Components/Filter/FilterModal';

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
