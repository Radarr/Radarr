function formatRuntime(minutes) {
  if (!minutes) {
    return '0m';
  }

  const movieHours = Math.floor(minutes / 60);
  const movieMinutes = (minutes <= 59) ? minutes : minutes % 60;
  const formattedRuntime = `${((movieHours > 0) ? `${movieHours}h ` : '') + movieMinutes}m`;

  return formattedRuntime;
}

export default formatRuntime;
