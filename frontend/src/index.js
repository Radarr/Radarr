import { createBrowserHistory } from 'history';
import React from 'react';
import { render } from 'react-dom';
import createAppStore from 'Store/createAppStore';
import { fetchTranslations } from 'Utilities/String/translate';

import './preload';
import './polyfills';
import 'Styles/globals.css';
import './index.css';

const history = createBrowserHistory();
const store = createAppStore(history);
const hasTranslationsError = !await fetchTranslations();
const { default: App } = await import('./App/App');

render(
  <App
    store={store}
    history={history}
    hasTranslationsError={hasTranslationsError}
  />,
  document.getElementById('root')
);
