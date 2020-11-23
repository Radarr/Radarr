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
    colorImpairedMode
  } = props;

  const iconsToShow = [];

  if (showCutoffUnmetIcon) {
    iconsToShow.push(
      <LegendIconItem
        name={translate('CutoffUnmet')}
        icon={icons.MOVIE_FILE}
        kind={kinds.WARNING}
        tooltip={translate('QualityOrLangCutoffHasNotBeenMet')}
      />
    );
  }

  return (
    <div className={styles.legend}>
      <div>
        <LegendItem
          status={translate('Unreleased')}
          tooltip={translate('MovieHasntReleasedYet')}
          colorImpairedMode={colorImpairedMode}
        />

        <LegendItem
          status={translate('Unmonitored')}
          tooltip={translate('MovieIsUnmonitored')}
          colorImpairedMode={colorImpairedMode}
        />
      </div>

      <div>
        <LegendItem
          status={translate('Downloading')}
          tooltip={translate('MovieIsDownloading')}
          colorImpairedMode={colorImpairedMode}
        />

        <LegendItem
          status={translate('Downloaded')}
          tooltip={translate('MovieWasDownloadedAndSorted')}
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
