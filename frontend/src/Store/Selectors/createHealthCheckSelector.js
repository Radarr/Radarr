import { createSelector } from 'reselect';

function createHealthCheckSelector() {
  return createSelector(
    (state) => state.system.health,
    (state) => state.app,
    (health, app) => {
      const items = [...health.items];

      if (!app.isConnected) {
        items.push({
          source: 'UI',
          type: 'warning',
          message: 'Could not connect to SignalR, UI won\'t update',
          wikiUrl: 'https://github.com/Radarr/Radarr/wiki/Health-Checks#could-not-connect-to-signalr'
        });
      }

      return items;
    }
  );
}

export default createHealthCheckSelector;
