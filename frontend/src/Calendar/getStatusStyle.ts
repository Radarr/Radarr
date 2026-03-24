function getStatusStyle(
  hasFile: boolean,
  downloading: boolean,
  isMonitored: boolean,
  isAvailable: boolean
) {
  if (downloading) {
    return 'queue';
  }

  if (hasFile && isMonitored) {
    return 'downloaded';
  }

  if (hasFile && !isMonitored) {
    return 'unmonitored';
  }

  if (isAvailable && isMonitored) {
    return 'missingMonitored';
  }

  if (!isMonitored) {
    return 'missingUnmonitored';
  }

  return 'continuing';
}

export default getStatusStyle;
