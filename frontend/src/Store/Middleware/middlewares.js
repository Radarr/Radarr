import { applyMiddleware, compose } from 'redux';
import ravenMiddleware from 'redux-raven-middleware';
import thunk from 'redux-thunk';
import { routerMiddleware } from 'react-router-redux';
import persistState from './persistState';

export default function(history) {
  const {
    analytics,
    branch,
    version,
    release,
    isProduction
  } = window.Sonarr;

  const dsn = isProduction ? 'https://b80ca60625b443c38b242e0d21681eb7@sentry.sonarr.tv/13' :
                            'https://8dbaacdfe2ff4caf97dc7945aecf9ace@sentry.sonarr.tv/12';

  const middlewares = [];

  if (analytics) {
    middlewares.push(ravenMiddleware(dsn, {
      environment: isProduction ? 'production' : 'development',
      release,
      tags: {
        branch,
        version
      }
    }));
  }

  middlewares.push(routerMiddleware(history));
  middlewares.push(thunk);

  return compose(
    applyMiddleware(...middlewares),
    persistState
  );
}
