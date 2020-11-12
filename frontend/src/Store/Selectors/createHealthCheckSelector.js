import { createSelector } from 'reselect';
import translate from 'Utilities/String/translate';

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
          message: translate('CouldNotConnectSignalR'),
          wikiUrl: 'https://wiki.servarr.com/Radarr_System#Could_not_connect_to_signalR'
        });
      }

      return items;
    }
  );
}

export default createHealthCheckSelector;
