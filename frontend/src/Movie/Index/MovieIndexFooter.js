import PropTypes from 'prop-types';
import React, { PureComponent } from 'react';
import formatBytes from 'Utilities/Number/formatBytes';
import DescriptionList from 'Components/DescriptionList/DescriptionList';
import DescriptionListItem from 'Components/DescriptionList/DescriptionListItem';
import styles from './MovieIndexFooter.css';

class MovieIndexFooter extends PureComponent {

  render() {
    const {
      movies
    } = this.props;

    const count = movies.length;
    let movieFiles = 0;
    let monitored = 0;
    let totalFileSize = 0;

    movies.forEach((s) => {
      const { statistics = {} } = s;

      const {
        sizeOnDisk = 0
      } = statistics;

      if (s.hasFile) {
        movieFiles += 1;
      }

      // if (s.status === 'ended') {
      //   ended++;
      // } else {
      //   continuing++;
      // }

      if (s.monitored) {
        monitored++;
      }

      totalFileSize += sizeOnDisk;
    });

    return (
      <div className={styles.footer}>
        <div>
          <div className={styles.legendItem}>
            <div className={styles.ended} />
            <div>Downloaded and Monitored</div>
          </div>

          <div className={styles.legendItem}>
            <div className={styles.availNotMonitored} />
            <div>Downloaded, but not Monitored</div>
          </div>

          <div className={styles.legendItem}>
            <div className={styles.missingMonitored} />
            <div>Missing, but not Monitored</div>
          </div>

          <div className={styles.legendItem}>
            <div className={styles.missingUnmonitored} />
            <div>Missing, Monitored and considered Available</div>
          </div>

          <div className={styles.legendItem}>
            <div className={styles.continuing} />
            <div>Unreleased</div>
          </div>
        </div>

        <div className={styles.statistics}>
          <DescriptionList>
            <DescriptionListItem
              title="Movies"
              data={count}
            />

            <DescriptionListItem
              title="Movie Files"
              data={movieFiles}
            />
          </DescriptionList>

          <DescriptionList>
            <DescriptionListItem
              title="Monitored"
              data={monitored}
            />

            <DescriptionListItem
              title="Unmonitored"
              data={count - monitored}
            />
          </DescriptionList>

          <DescriptionList>
            <DescriptionListItem
              title="Total File Size"
              data={formatBytes(totalFileSize)}
            />
          </DescriptionList>
        </div>
      </div>
    );
  }
}

MovieIndexFooter.propTypes = {
  movies: PropTypes.arrayOf(PropTypes.object).isRequired
};

export default MovieIndexFooter;
