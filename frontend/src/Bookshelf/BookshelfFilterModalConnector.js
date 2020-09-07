import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import FilterModal from 'Components/Filter/FilterModal';
import { setBookshelfFilter } from 'Store/Actions/bookshelfActions';

function createMapStateToProps() {
  return createSelector(
    (state) => state.authors.items,
    (state) => state.bookshelf.filterBuilderProps,
    (sectionItems, filterBuilderProps) => {
      return {
        sectionItems,
        filterBuilderProps,
        customFilterType: 'bookshelf'
      };
    }
  );
}

const mapDispatchToProps = {
  dispatchSetFilter: setBookshelfFilter
};

export default connect(createMapStateToProps, mapDispatchToProps)(FilterModal);
