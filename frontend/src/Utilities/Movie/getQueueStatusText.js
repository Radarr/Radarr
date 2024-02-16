import titleCase from 'Utilities/String/titleCase';
import translate from 'Utilities/String/translate';

export default function getQueueStatusText(queueStatus, queueState) {
  if (!queueStatus) {
    return;
  }

  let status = translate('Downloading');

  switch (true) {
    case queueStatus !== 'completed':
      switch (queueStatus) {
        case 'queue':
        case 'paused':
        case 'failed':
        case 'downloading':
          status = titleCase(queueStatus);
          break;
        case 'delay':
        case 'downloadClientUnavailable':
          status = translate('Pending');
          break;
        case 'warning':
          status = translate('Error');
          break;
        default:
      }
      break;

    case queueStatus === 'completed':
      switch (queueState) {
        case 'importPending':
          status = translate('Pending');
          break;
        case 'importing':
          status = translate('Importing');
          break;
        case 'failedPending':
          status = translate('Waiting');
          break;
        default:
      }
      break;

    default:
  }

  return status;
}
