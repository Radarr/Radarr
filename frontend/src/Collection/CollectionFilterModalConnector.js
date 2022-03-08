import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import FilterModal from 'Components/Filter/FilterModal';
import { setMovieCollectionsFilter } from 'Store/Actions/movieCollectionActions';

function createMapStateToProps() {
  return createSelector(
    (state) => state.movieCollections.items,
    (state) => state.movieCollections.filterBuilderProps,
    (sectionItems, filterBuilderProps) => {
      return {
        sectionItems,
        filterBuilderProps,
        customFilterType: 'movieCollections'
      };
    }
  );
}

const mapDispatchToProps = {
  dispatchSetFilter: setMovieCollectionsFilter
};

export default connect(createMapStateToProps, mapDispatchToProps)(FilterModal);
