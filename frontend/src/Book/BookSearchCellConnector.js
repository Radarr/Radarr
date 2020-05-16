import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { isCommandExecuting } from 'Utilities/Command';
import createAuthorSelector from 'Store/Selectors/createAuthorSelector';
import createCommandsSelector from 'Store/Selectors/createCommandsSelector';
import { executeCommand } from 'Store/Actions/commandActions';
import * as commandNames from 'Commands/commandNames';
import BookSearchCell from './BookSearchCell';

function createMapStateToProps() {
  return createSelector(
    (state, { bookId }) => bookId,
    createAuthorSelector(),
    createCommandsSelector(),
    (bookId, author, commands) => {
      const isSearching = commands.some((command) => {
        const bookSearch = command.name === commandNames.BOOK_SEARCH;

        if (!bookSearch) {
          return false;
        }

        return (
          isCommandExecuting(command) &&
          command.body.bookIds.indexOf(bookId) > -1
        );
      });

      return {
        authorMonitored: author.monitored,
        authorType: author.authorType,
        isSearching
      };
    }
  );
}

function createMapDispatchToProps(dispatch, props) {
  return {
    onSearchPress(name, path) {
      dispatch(executeCommand({
        name: commandNames.BOOK_SEARCH,
        bookIds: [props.bookId]
      }));
    }
  };
}

export default connect(createMapStateToProps, createMapDispatchToProps)(BookSearchCell);
