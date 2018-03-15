import PropTypes from 'prop-types';
import React from 'react';
import formatBytes from 'Utilities/Number/formatBytes';
import DescriptionList from 'Components/DescriptionList/DescriptionList';
import DescriptionListItem from 'Components/DescriptionList/DescriptionListItem';
import styles from './ArtistIndexFooter.css';

function ArtistIndexFooter({ artist }) {
  const count = artist.length;
  let tracks = 0;
  let trackFiles = 0;
  let ended = 0;
  let continuing = 0;
  let monitored = 0;
  let totalFileSize = 0;

  artist.forEach((s) => {
    tracks += s.trackCount || 0;
    trackFiles += s.trackFileCount || 0;

    if (s.status === 'ended') {
      ended++;
    } else {
      continuing++;
    }

    if (s.monitored) {
      monitored++;
    }

    totalFileSize += s.statistics.sizeOnDisk || 0;
  });

  return (
    <div className={styles.footer}>
      <div>
        <div className={styles.legendItem}>
          <div className={styles.continuing} />
          <div>Continuing (All tracks downloaded)</div>
        </div>

        <div className={styles.legendItem}>
          <div className={styles.ended} />
          <div>Ended (All tracks downloaded)</div>
        </div>

        <div className={styles.legendItem}>
          <div className={styles.missingMonitored} />
          <div>Missing Tracks (Artist monitored)</div>
        </div>

        <div className={styles.legendItem}>
          <div className={styles.missingUnmonitored} />
          <div>Missing Tracks (Artist not monitored)</div>
        </div>
      </div>

      <div className={styles.statistics}>
        <DescriptionList>
          <DescriptionListItem
            title="Artist"
            data={count}
          />

          <DescriptionListItem
            title="Ended"
            data={ended}
          />

          <DescriptionListItem
            title="Continuing"
            data={continuing}
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
            title="Tracks"
            data={tracks}
          />

          <DescriptionListItem
            title="Files"
            data={trackFiles}
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

ArtistIndexFooter.propTypes = {
  artist: PropTypes.arrayOf(PropTypes.object).isRequired
};

export default ArtistIndexFooter;
