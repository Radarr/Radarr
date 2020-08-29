function formatRuntime(minutes, format) {
  if (!minutes) {
    return (format === 'hoursMinutes') ? '0m' : '0 mins';
  }

  if (format === 'minutes') {
    return `${minutes} mins`;
  }

  const movieHours = Math.floor(minutes / 60);
  const movieMinutes = (minutes <= 59) ? minutes : minutes % 60;
  return `${((movieHours > 0) ? `${movieHours}h ` : '') + movieMinutes}m`;
}

export default formatRuntime;
