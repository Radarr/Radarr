import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import FilterModal from 'Components/Filter/FilterModal';
import { setAuthorEditorFilter } from 'Store/Actions/authorEditorActions';

function createMapStateToProps() {
  return createSelector(
    (state) => state.authors.items,
    (state) => state.authorEditor.filterBuilderProps,
    (sectionItems, filterBuilderProps) => {
      return {
        sectionItems,
        filterBuilderProps,
        customFilterType: 'authorEditor'
      };
    }
  );
}

const mapDispatchToProps = {
  dispatchSetFilter: setAuthorEditorFilter
};

export default connect(createMapStateToProps, mapDispatchToProps)(FilterModal);
