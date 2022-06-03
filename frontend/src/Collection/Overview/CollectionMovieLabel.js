import classNames from 'classnames';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import MonitorToggleButton from 'Components/MonitorToggleButton';
import getStatusStyle from 'Utilities/Movie/getStatusStyle';
import translate from 'Utilities/String/translate';
import styles from './CollectionMovieLabel.css';

class CollectionMovieLabel extends Component {
  //
  // Render

  render() {
    const {
      id,
      title,
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
            {
              title
            }
          </span>
        </div>

        {
          id &&
            <div
              className={classNames(
                styles.movieStatus,
                styles[getStatusStyle(status, monitored, hasFile, isAvailable, 'kinds')]
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
  statistics: {
    episodeFileCount: 0,
    totalEpisodeCount: 0,
    percentOfEpisodes: 0
  }
};

export default CollectionMovieLabel;
