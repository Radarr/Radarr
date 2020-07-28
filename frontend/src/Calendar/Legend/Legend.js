import PropTypes from 'prop-types';
import React from 'react';
import { icons, kinds } from 'Helpers/Props';
import LegendIconItem from './LegendIconItem';
import LegendItem from './LegendItem';
import styles from './Legend.css';

function Legend(props) {
  const {
    showCutoffUnmetIcon,
    colorImpairedMode
  } = props;

  const iconsToShow = [];

  if (showCutoffUnmetIcon) {
    iconsToShow.push(
      <LegendIconItem
        name="Cutoff Not Met"
        icon={icons.MOVIE_FILE}
        kind={kinds.WARNING}
        tooltip="Quality or language cutoff has not been met"
      />
    );
  }

  return (
    <div className={styles.legend}>
      <div>
        <LegendItem
          status="unreleased"
          tooltip="Movie hasn't released yet"
          colorImpairedMode={colorImpairedMode}
        />

        <LegendItem
          status="unmonitored"
          tooltip="Movie is unmonitored"
          colorImpairedMode={colorImpairedMode}
        />
      </div>

      <div>
        <LegendItem
          status="downloading"
          tooltip="Movie is currently downloading"
          colorImpairedMode={colorImpairedMode}
        />

        <LegendItem
          status="downloaded"
          tooltip="Movie was downloaded and sorted"
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
  showCutoffUnmetIcon: PropTypes.bool.isRequired,
  colorImpairedMode: PropTypes.bool.isRequired
};

export default Legend;
