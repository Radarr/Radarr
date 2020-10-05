import { kinds } from 'Helpers/Props';

function getProgressBarKind(status, monitored, hasFile, isAvailable, queue = false) {
  if (queue) {
    return kinds.QUEUE;
  }

  if (hasFile && monitored) {
    return kinds.SUCCESS;
  }

  if (hasFile && !monitored) {
    return kinds.DEFAULT;
  }

  if (isAvailable && monitored) {
    return kinds.DANGER;
  }

  if (!monitored) {
    return kinds.WARNING;
  }

  return kinds.PRIMARY;
}

export default getProgressBarKind;
