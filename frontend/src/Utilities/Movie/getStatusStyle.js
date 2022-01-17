import { kinds } from 'Helpers/Props';

function getStatusStyle(status, monitored, hasFile, isAvailable, returnType, queue = false) {
  if (queue) {
    return returnType === 'kinds' ? kinds.QUEUE : 'queue';
  }

  if (hasFile && monitored) {
    return returnType === 'kinds' ? kinds.SUCCESS : 'downloaded';
  }

  if (hasFile && !monitored) {
    return returnType === 'kinds' ? kinds.DEFAULT : 'unreleased';
  }

  if (isAvailable && monitored) {
    return returnType === 'kinds' ? kinds.DANGER : 'missingMonitored';
  }

  if (!monitored) {
    return returnType === 'kinds' ? kinds.WARNING : 'missingUnmonitored';
  }

  return returnType === 'kinds' ? kinds.PRIMARY : 'continuing';
}

export default getStatusStyle;
