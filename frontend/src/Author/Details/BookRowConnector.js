/* eslint max-params: 0 */
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import createAuthorSelector from 'Store/Selectors/createAuthorSelector';
import createBookFileSelector from 'Store/Selectors/createBookFileSelector';
import BookRow from './BookRow';

function createMapStateToProps() {
  return createSelector(
    createAuthorSelector(),
    createBookFileSelector(),
    (author = {}, bookFile) => {
      return {
        authorMonitored: author.monitored,
        bookFilePath: bookFile ? bookFile.path : null
      };
    }
  );
}
export default connect(createMapStateToProps)(BookRow);
