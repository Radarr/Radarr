import { createSelector } from 'reselect';
import { findCommand, isCommandExecuting } from 'Utilities/Command';
import createCommandsSelector from './createCommandsSelector';

function createCommandExecutingSelector(name, contraints = {}) {
  return createSelector(
    createCommandsSelector(),
    (commands) => {
      const command = findCommand(commands, { name, ...contraints });
      return isCommandExecuting(command);
    }
  );
}

export default createCommandExecutingSelector;
