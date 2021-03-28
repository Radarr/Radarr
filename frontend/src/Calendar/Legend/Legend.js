import PropTypes from 'prop-types';
import React from 'react';
import { icons, kinds } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import LegendIconItem from './LegendIconItem';
import LegendItem from './LegendItem';
import styles from './Legend.css';

function Legend(props) {
  const {
    showCutoffUnmetIcon,
    fullColorEvents,
    colorImpairedMode
  } = props;

  const iconsToShow = [];

  if (showCutoffUnmetIcon) {
    iconsToShow.push(
      <LegendIconItem
        name={translate('CutoffUnmet')}
        icon={icons.MOVIE_FILE}
        kind={fullColorEvents ? kinds.DEFAULT : kinds.WARNING}
        tooltip={translate('QualityOrLangCutoffHasNotBeenMet')}
      />
    );
  }

  return (
    <div className={styles.legend}>
      <div>
        <LegendItem
          style='ended'
          name={translate('DownloadedAndMonitored')}
          fullColorEvents={fullColorEvents}
          colorImpairedMode={colorImpairedMode}
        />

        <LegendItem
          style='availNotMonitored'
          name={translate('DownloadedButNotMonitored')}
          fullColorEvents={fullColorEvents}
          colorImpairedMode={colorImpairedMode}
        />
      </div>

      <div>
        <LegendItem
          style='missingMonitored'
          name={translate('MissingMonitoredAndConsideredAvailable')}
          fullColorEvents={fullColorEvents}
          colorImpairedMode={colorImpairedMode}
        />

        <LegendItem
          style='missingUnmonitored'
          name={translate('MissingNotMonitored')}
          fullColorEvents={fullColorEvents}
          colorImpairedMode={colorImpairedMode}
        />
      </div>

      <div>
        <LegendItem
          style='queue'
          name={translate('Queued')}
          fullColorEvents={fullColorEvents}
          colorImpairedMode={colorImpairedMode}
        />

        <LegendItem
          style='continuing'
          name={translate('Unreleased')}
          fullColorEvents={fullColorEvents}
          colorImpairedMode={colorImpairedMode}
        />
      </div>
      {
        iconsToShow.length > 0 &&
          <div>
            {iconsToShow[0]}
          </div>
      }
    </div>
  );
}

Legend.propTypes = {
  view: PropTypes.string.isRequired,
  showCutoffUnmetIcon: PropTypes.bool.isRequired,
  fullColorEvents: PropTypes.bool.isRequired,
  colorImpairedMode: PropTypes.bool.isRequired
};

export default Legend;
