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
      onMonitorTogglePress,
      isSaving,
      statistics
    } = this.props;

    const { movieFileCount } = statistics;

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
                styles[getStatusStyle(status, monitored, movieFileCount > 0, isAvailable, 'kinds')]
              )}
            >
              {
                movieFileCount > 0 ? translate('Downloaded') : translate('Missing')
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
  statistics: PropTypes.object.isRequired,
  isAvailable: PropTypes.bool,
  monitored: PropTypes.bool,
  isSaving: PropTypes.bool.isRequired,
  movieFile: PropTypes.object,
  movieFileId: PropTypes.number,
  onMonitorTogglePress: PropTypes.func.isRequired
};

CollectionMovieLabel.defaultProps = {
  isSaving: false,
  statistics: {
    movieFileCount: 0
  }
};

export default CollectionMovieLabel;
