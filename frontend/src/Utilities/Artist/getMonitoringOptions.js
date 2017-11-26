import _ from 'lodash';

function getMonitoringOptions(monitor) {
  const monitoringOptions = {
    ignoreAlbumsWithFiles: false,
    ignoreAlbumsWithoutFiles: false,
    monitored: true
  };

  switch (monitor) {
    case 'future':
      monitoringOptions.ignoreAlbumsWithFiles = true;
      monitoringOptions.ignoreAlbumsWithoutFiles = true;
      break;
    case 'missing':
      monitoringOptions.ignoreAlbumsWithFiles = true;
      break;
    case 'existing':
      monitoringOptions.ignoreAlbumsWithoutFiles = true;
      break;
    case 'none':
      monitoringOptions.monitored = false;
      break;
    default:
      break;
  }

  return {
    options: monitoringOptions
  };
}

export default getMonitoringOptions;
