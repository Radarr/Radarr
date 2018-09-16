function getMonitoringOptions(monitor) {
  const monitoringOptions = {
    selectedOption: 0,
    monitored: true
  };

  switch (monitor) {
    case 'future':
      monitoringOptions.selectedOption = 1;
      break;
    case 'missing':
      monitoringOptions.selectedOption = 2;
      break;
    case 'existing':
      monitoringOptions.selectedOption = 3;
      break;
    case 'first':
      monitoringOptions.selectedOption = 5;
      break;
    case 'latest':
      monitoringOptions.selectedOption = 4;
      break;
    case 'none':
      monitoringOptions.monitored = false;
      monitoringOptions.selectedOption = 6;
      break;
    default:
      monitoringOptions.selectedOption = 0;
      break;
  }

  return {
    options: monitoringOptions
  };
}

export default getMonitoringOptions;
