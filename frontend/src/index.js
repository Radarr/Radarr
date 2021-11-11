import { createBrowserHistory } from 'history';
import React from 'react';
import { render } from 'react-dom';
import ThemeSelector from 'App/ThemeSelector';
import createAppStore from 'Store/createAppStore';
import App from './App/App';

import './preload';
import './polyfills';
import 'Styles/globals.css';
import './index.css';

const history = createBrowserHistory();
const store = createAppStore(history);

render(
  <ThemeSelector>
    <App
      store={store}
      history={history}
    />
  </ThemeSelector>,
  document.getElementById('root')
);
