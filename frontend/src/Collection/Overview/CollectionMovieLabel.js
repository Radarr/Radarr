import classNames from 'classnames';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import MonitorToggleButton from 'Components/MonitorToggleButton';
import getProgressBarKind from 'Utilities/Movie/getProgressBarKind';
import translate from 'Utilities/String/translate';
import styles from './CollectionMovieLabel.css';

class CollectionMovieLabel extends Component {
  //
  // Render

  render() {
    const {
      id,
      title,
      year,
      status,
      monitored,
      isAvailable,
      hasFile,
      onMonitorTogglePress,
      isSaving
    } = this.props;

    return (
      <div className={styles.movie}>
        <div className={styles.movieTitle}>
          {
            id &&
              <MonitorToggleButton
                monitored={monitored}
                isSaving={isSaving}
                onPress={onMonitorTogglePress}
              />
          }

          <span>
            {title} {year > 0 ? `(${year})` : ''}
          </span>
        </div>

        {
          id &&
            <div
              className={classNames(
                styles.movieStatus,
                styles[getProgressBarKind(status, monitored, hasFile, isAvailable)]
              )}
            >
              {
                hasFile ? translate('Downloaded') : translate('Missing')
              }
            </div>
        }
      </div>
    );
  }
}

CollectionMovieLabel.propTypes = {
  id: PropTypes.number,
  title: PropTypes.string.isRequired,
  year: PropTypes.number.isRequired,
  status: PropTypes.string,
  isAvailable: PropTypes.bool,
  monitored: PropTypes.bool,
  hasFile: PropTypes.bool,
  isSaving: PropTypes.bool.isRequired,
  movieFile: PropTypes.object,
  movieFileId: PropTypes.number,
  onMonitorTogglePress: PropTypes.func.isRequired
};

CollectionMovieLabel.defaultProps = {
  isSaving: false,
  statistics: {}
};

export default CollectionMovieLabel;
