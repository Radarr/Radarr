import { kinds } from 'Helpers/Props';

function getProgressBarKind(status, monitored, hasFile, queue = false) {
  if (queue) {
    return kinds.QUEUE;
  }

  if (status === 'announced') {
    return kinds.PRIMARY;
  }

  if (hasFile && monitored) {
    return kinds.SUCCESS;
  }

  if (hasFile && !monitored) {
    return kinds.DEFAULT;
  }

  if (monitored) {
    return kinds.DANGER;
  }

  return kinds.WARNING;
}

export default getProgressBarKind;
