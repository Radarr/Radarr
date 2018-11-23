import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { setSeriesEditorFilter } from 'Store/Actions/movieEditorActions';
import FilterModal from 'Components/Filter/FilterModal';

function createMapStateToProps() {
  return createSelector(
    (state) => state.movies.items,
    (state) => state.moviesEditor.filterBuilderProps,
    (sectionItems, filterBuilderProps) => {
      return {
        sectionItems,
        filterBuilderProps,
        customFilterType: 'movieEditor'
      };
    }
  );
}

const mapDispatchToProps = {
  dispatchSetFilter: setSeriesEditorFilter
};

export default connect(createMapStateToProps, mapDispatchToProps)(FilterModal);
