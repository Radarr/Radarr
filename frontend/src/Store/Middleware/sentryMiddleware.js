import _ from 'lodash';
import Raven from 'raven-js';
import createRavenMiddleware from 'raven-for-redux';
import parseUrl from 'Utilities/String/parseUrl';

function cleanseUrl(url) {
  const properties = parseUrl(url);

  return `${properties.pathname}${properties.search}`;
}

function cleanseData(data) {
  const result = _.cloneDeep(data);

  result.culprit = cleanseUrl(result.culprit);
  result.request.url = cleanseUrl(result.request.url);

  return result;
}

export default function sentryMiddleware() {
  const {
    analytics,
    branch,
    version,
    release,
    isProduction
  } = window.Lidarr;

  if (!analytics) {
    return;
  }

  const dsn = isProduction ? 'https://c3a5b33e08de4e18b7d0505e942dbc95@sentry.io/216290' :
    'https://c3a5b33e08de4e18b7d0505e942dbc95@sentry.io/216290';

  Raven.config(dsn).install();

  return createRavenMiddleware(Raven, {
    environment: isProduction ? 'production' : 'development',
    release,
    tags: {
      branch,
      version
    },
    dataCallback: cleanseData
  });
}
