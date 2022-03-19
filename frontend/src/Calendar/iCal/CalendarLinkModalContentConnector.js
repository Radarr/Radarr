import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import createReleaseTypesSelector from 'Store/Selectors/createReleaseTypesSelector';
import createTagsSelector from 'Store/Selectors/createTagsSelector';
import CalendarLinkModalContent from './CalendarLinkModalContent';

function createMapStateToProps() {
  return createSelector(
    createTagsSelector(),
    createReleaseTypesSelector(),
    (tagList, releaseTypes) => {
      return {
        tagList,
        releaseTypes
      };
    }
  );
}

export default connect(createMapStateToProps)(CalendarLinkModalContent);
