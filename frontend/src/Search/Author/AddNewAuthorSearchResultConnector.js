import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import createDimensionsSelector from 'Store/Selectors/createDimensionsSelector';
import createExistingAuthorSelector from 'Store/Selectors/createExistingAuthorSelector';
import AddNewAuthorSearchResult from './AddNewAuthorSearchResult';

function createMapStateToProps() {
  return createSelector(
    createExistingAuthorSelector(),
    createDimensionsSelector(),
    (isExistingAuthor, dimensions) => {
      return {
        isExistingAuthor,
        isSmallScreen: dimensions.isSmallScreen
      };
    }
  );
}

export default connect(createMapStateToProps)(AddNewAuthorSearchResult);
