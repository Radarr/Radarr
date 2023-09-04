import translate from 'Utilities/String/translate';

function formatRuntime(runtime: number, format = 'hoursMinutes') {
  if (!runtime) {
    return format === 'hoursMinutes' ? '0m' : '0 mins';
  }

  if (format === 'minutes') {
    return `${runtime} mins`;
  }

  const hours = Math.floor(runtime / 60);
  const minutes = runtime % 60;
  const result = [];

  if (hours) {
    result.push(translate('FormatRuntimeHours', { hours }));
  }

  if (minutes) {
    result.push(translate('FormatRuntimeMinutes', { minutes }));
  }

  return result.join(' ');
}

export default formatRuntime;
