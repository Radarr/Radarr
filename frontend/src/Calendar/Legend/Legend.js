import PropTypes from 'prop-types';
import React from 'react';
import { icons, kinds } from 'Helpers/Props';
import LegendItem from './LegendItem';
import LegendIconItem from './LegendIconItem';
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
        icon={icons.TRACK_FILE}
        kind={kinds.WARNING}
        tooltip="Quality cutoff has not been met"
      />
    );
  }

  return (
    <div className={styles.legend}>
      <div>
        <LegendItem
          status="downloading"
          tooltip="Album is currently downloading"
          colorImpairedMode={colorImpairedMode}
        />

        <LegendItem
          status="downloaded"
          tooltip="Album was downloaded and sorted"
          colorImpairedMode={colorImpairedMode}
        />
      </div>

      <div>
        <LegendItem
          status="unreleased"
          tooltip="Album hasn't released yet"
          colorImpairedMode={colorImpairedMode}
        />

        <LegendItem
          status="partial"
          tooltip="Album was partially downloaded"
          colorImpairedMode={colorImpairedMode}
        />
      </div>

      <div>
        <LegendItem
          status="unmonitored"
          tooltip="Album is unmonitored"
          colorImpairedMode={colorImpairedMode}
        />

        <LegendItem
          status="missing"
          tooltip="Track file has not been found"
          colorImpairedMode={colorImpairedMode}
        />
      </div>

      <div>
        {iconsToShow[0]}
      </div>

      {
        iconsToShow.length > 1 &&
          <div>
            {iconsToShow[1]}
            {iconsToShow[2]}
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
