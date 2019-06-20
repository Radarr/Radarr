import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { setReleasesFilter } from 'Store/Actions/releaseActions';
import FilterModal from 'Components/Filter/FilterModal';

function createMapStateToProps() {
  return createSelector(
    (state) => state.releases.items,
    (state) => state.releases.filterBuilderProps,
    (sectionItems, filterBuilderProps) => {
      return {
        sectionItems,
        filterBuilderProps,
        customFilterType: 'releases'
      };
    }
  );
}

function createMapDispatchToProps(dispatch, props) {
  return {
    dispatchSetFilter(payload) {
      const action = setReleasesFilter;
      dispatch(action(payload));
    }
  };
}

export default connect(createMapStateToProps, createMapDispatchToProps)(FilterModal);
