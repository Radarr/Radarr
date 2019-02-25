import { applyMiddleware, compose } from 'redux';
import thunk from 'redux-thunk';
import { routerMiddleware } from 'react-router-redux';
import createSentryMiddleware from './createSentryMiddleware';
import createPersistState from './createPersistState';

export default function(history) {
  const middlewares = [];
  const sentryMiddleware = createSentryMiddleware();

  if (sentryMiddleware) {
    middlewares.push(sentryMiddleware);
  }

  middlewares.push(routerMiddleware(history));
  middlewares.push(thunk);

  // eslint-disable-next-line no-underscore-dangle
  const composeEnhancers = window.__REDUX_DEVTOOLS_EXTENSION_COMPOSE__ || compose;

  return composeEnhancers(
    applyMiddleware(...middlewares),
    createPersistState()
  );
}
