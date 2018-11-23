import { kinds } from 'Helpers/Props';

function getProgressBarKind(status, monitored, hasFile) {
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
