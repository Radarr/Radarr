function getStatusStyle(hasFile, downloading, isMonitored, isAvailable) {
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
