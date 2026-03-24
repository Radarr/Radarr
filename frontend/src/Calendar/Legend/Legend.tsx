import React from 'react';
import { useSelector } from 'react-redux';
import AppState from 'App/State/AppState';
import { icons, kinds } from 'Helpers/Props';
import createUISettingsSelector from 'Store/Selectors/createUISettingsSelector';
import translate from 'Utilities/String/translate';
import LegendIconItem from './LegendIconItem';
import LegendItem from './LegendItem';
import styles from './Legend.css';

function Legend() {
  const view = useSelector((state: AppState) => state.calendar.view);
  const { showCutoffUnmetIcon, fullColorEvents } = useSelector(
    (state: AppState) => state.calendar.options
  );
  const { enableColorImpairedMode } = useSelector(createUISettingsSelector());

  const iconsToShow = [];
  const isAgendaView = view === 'agenda';

  if (showCutoffUnmetIcon) {
    iconsToShow.push(
      <LegendIconItem
        name={translate('CutoffNotMet')}
        icon={icons.MOVIE_FILE}
        kind={kinds.WARNING}
        fullColorEvents={fullColorEvents}
        tooltip={translate('QualityCutoffNotMet')}
      />
    );
  }

  return (
    <div className={styles.legend}>
      <div>
        <LegendItem
          status="downloaded"
          name={translate('DownloadedAndMonitored')}
          isAgendaView={isAgendaView}
          fullColorEvents={fullColorEvents}
          colorImpairedMode={enableColorImpairedMode}
        />

        <LegendItem
          status="unmonitored"
          name={translate('DownloadedButNotMonitored')}
          isAgendaView={isAgendaView}
          fullColorEvents={fullColorEvents}
          colorImpairedMode={enableColorImpairedMode}
        />
      </div>

      <div>
        <LegendItem
          status="missingMonitored"
          name={translate('MissingMonitoredAndConsideredAvailable')}
          isAgendaView={isAgendaView}
          fullColorEvents={fullColorEvents}
          colorImpairedMode={enableColorImpairedMode}
        />

        <LegendItem
          status="missingUnmonitored"
          name={translate('MissingNotMonitored')}
          isAgendaView={isAgendaView}
          fullColorEvents={fullColorEvents}
          colorImpairedMode={enableColorImpairedMode}
        />
      </div>

      <div>
        <LegendItem
          status="queue"
          name={translate('Queued')}
          isAgendaView={isAgendaView}
          fullColorEvents={fullColorEvents}
          colorImpairedMode={enableColorImpairedMode}
        />

        <LegendItem
          status="continuing"
          name={translate('Unreleased')}
          isAgendaView={isAgendaView}
          fullColorEvents={fullColorEvents}
          colorImpairedMode={enableColorImpairedMode}
        />
      </div>

      {iconsToShow.length > 0 ? <div>{iconsToShow[0]}</div> : null}
    </div>
  );
}

export default Legend;
