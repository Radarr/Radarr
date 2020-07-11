
function getStatusStyle(hasFile, downloading, isAvailable, isMonitored) {

  if (hasFile) {
    return 'downloaded';
  }

  if (downloading) {
    return 'downloading';
  }

  if (!isMonitored) {
    return 'unmonitored';
  }

  if (isAvailable && !hasFile) {
    return 'missing';
  }

  return 'unreleased';
}

export default getStatusStyle;
