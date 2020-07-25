import titleCase from 'Utilities/String/titleCase';
import translate from 'Utilities/String/translate';

export default function getQueueStatusText(queueStatus, queueState) {
  if (!queueStatus) {
    return;
  }

  let statusLong = translate('Downloading');
  let statusShort = translate('Downloading');

  switch (true) {
    case queueStatus !== 'completed':
      switch (queueStatus) {
        case 'queue':
        case 'paused':
        case 'failed':
          statusLong = `${translate('Downloading')}: ${translate(titleCase(queueStatus))}`;
          statusShort = titleCase(queueStatus);
          break;
        case 'delay':
          statusLong = `${translate('Downloading')}: ${translate('Pending')}`;
          statusShort = translate('Pending');
          break;
        case 'DownloadClientUnavailable':
        case 'warning':
          statusLong = `${translate('Downloading')}: ${translate('Error')}`;
          statusShort = translate('Error');
          break;
        case 'downloading':
          statusLong = titleCase(queueStatus);
          statusShort = titleCase(queueStatus);
          break;
        default:
      }
      break;

    case queueStatus === 'completed':
      switch (queueState) {
        case 'importPending':
          statusLong = `${translate('Downloaded')}: ${translate('Pending')}`;
          statusShort = translate('Downloaded');
          break;
        case 'importing':
          statusLong = `${translate('Downloaded')}: ${translate('Importing')}`;
          statusShort = translate('Downloaded');
          break;
        case 'failedPending':
          statusLong = `${translate('Downloaded')}: ${translate('Waiting')}`;
          statusShort = translate('Downloaded');
          break;
        default:
      }
      break;

    default:
  }

  const result = { longText: statusLong, shortText: statusShort };
  return result;
}
