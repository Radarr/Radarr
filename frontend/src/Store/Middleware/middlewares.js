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

  const dsn = isProduction ? 'https://c3a5b33e08de4e18b7d0505e942dbc95:e35e6d535b034995a6896022c6bfed04@sentry.io/216290' :
                            'https://c3a5b33e08de4e18b7d0505e942dbc95:e35e6d535b034995a6896022c6bfed04@sentry.io/216290';

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
