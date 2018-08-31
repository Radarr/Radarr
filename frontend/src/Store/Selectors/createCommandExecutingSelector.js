import { createSelector } from 'reselect';
import { findCommand, isCommandExecuting } from 'Utilities/Command';
import createCommandsSelector from './createCommandsSelector';

function createCommandExecutingSelector(name, contraints = {}) {
  return createSelector(
    createCommandsSelector(),
    (commands) => {
      return isCommandExecuting(findCommand(commands, { name, ...contraints }));
    }
  );
}

export default createCommandExecutingSelector;
