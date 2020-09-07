import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import FilterModal from 'Components/Filter/FilterModal';
import { setAuthorFilter } from 'Store/Actions/authorIndexActions';

function createMapStateToProps() {
  return createSelector(
    (state) => state.authors.items,
    (state) => state.authorIndex.filterBuilderProps,
    (sectionItems, filterBuilderProps) => {
      return {
        sectionItems,
        filterBuilderProps,
        customFilterType: 'authorIndex'
      };
    }
  );
}

const mapDispatchToProps = {
  dispatchSetFilter: setAuthorFilter
};

export default connect(createMapStateToProps, mapDispatchToProps)(FilterModal);
