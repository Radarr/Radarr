import classNames from 'classnames';
import PropTypes from 'prop-types';
import React, { PureComponent } from 'react';
import DescriptionList from 'Components/DescriptionList/DescriptionList';
import DescriptionListItem from 'Components/DescriptionList/DescriptionListItem';
import formatBytes from 'Utilities/Number/formatBytes';
import styles from './MovieIndexFooter.css';

class MovieIndexFooter extends PureComponent {

  render() {
    const {
      movies,
      colorImpairedMode
    } = this.props;

    const count = movies.length;
    let movieFiles = 0;
    let monitored = 0;
    let totalFileSize = 0;

    movies.forEach((s) => {

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

      totalFileSize += s.sizeOnDisk;
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
            <div className={classNames(
              styles.missingMonitored,
              colorImpairedMode && 'colorImpaired'
            )}
            />
            <div>Missing, Monitored and considered Available</div>
          </div>

          <div className={styles.legendItem}>
            <div className={classNames(
              styles.missingUnmonitored,
              colorImpairedMode && 'colorImpaired'
            )}
            />
            <div>Missing, not Monitored</div>
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
  movies: PropTypes.arrayOf(PropTypes.object).isRequired,
  colorImpairedMode: PropTypes.bool.isRequired
};

export default MovieIndexFooter;
