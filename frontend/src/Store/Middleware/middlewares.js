import { applyMiddleware, compose } from 'redux';
import thunk from 'redux-thunk';
import { routerMiddleware } from 'react-router-redux';
import sentryMiddleware from './sentryMiddleware';
import persistState from './persistState';

export default function(history) {
  const middlewares = [];
  const ravenMiddleware = sentryMiddleware();

  if (ravenMiddleware) {
    middlewares.push(ravenMiddleware);
  }

  middlewares.push(routerMiddleware(history));
  middlewares.push(thunk);

  // eslint-disable-next-line no-underscore-dangle
  const composeEnhancers = window.__REDUX_DEVTOOLS_EXTENSION_COMPOSE__ || compose;

  return composeEnhancers(
    applyMiddleware(...middlewares),
    persistState
  );
}
