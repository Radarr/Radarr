import PropTypes from 'prop-types';
import React, { Component } from 'react';
import Link from 'Components/Link/Link';
import MonitorToggleButton from 'Components/MonitorToggleButton';
import styles from './MovieCollectionLabel.css';

class MovieCollectionLabel extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      hasPosterError: false
    };
  }

  render() {
    const {
      tmdbId,
      title,
      monitored,
      onMonitorTogglePress
    } = this.props;

    return (
      <div>
        <MonitorToggleButton
          className={styles.monitorToggleButton}
          monitored={monitored}
          size={15}
          onPress={onMonitorTogglePress}
        />
        <Link
          to={{
            pathname: '/collections',
            state: { navigateToId: tmdbId }
          }}
          className={styles.titleLink}
        >
          {title}
        </Link>
      </div>
    );
  }
}

MovieCollectionLabel.propTypes = {
  title: PropTypes.string.isRequired,
  monitored: PropTypes.bool.isRequired,
  onMonitorTogglePress: PropTypes.func.isRequired,
  tmdbId: PropTypes.string.isRequired
};

export default MovieCollectionLabel;
