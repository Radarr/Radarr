import PropTypes from 'prop-types';
import React from 'react';
import LegendItem from './LegendItem';
import styles from './Legend.css';

function Legend({ colorImpairedMode }) {
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
    </div>
  );
}

Legend.propTypes = {
  colorImpairedMode: PropTypes.bool.isRequired
};

export default Legend;
