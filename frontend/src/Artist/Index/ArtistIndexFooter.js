import PropTypes from 'prop-types';
import React, { PureComponent } from 'react';
import classNames from 'classnames';
import formatBytes from 'Utilities/Number/formatBytes';
import { ColorImpairedConsumer } from 'App/ColorImpairedContext';
import DescriptionList from 'Components/DescriptionList/DescriptionList';
import DescriptionListItem from 'Components/DescriptionList/DescriptionListItem';
import styles from './ArtistIndexFooter.css';

class ArtistIndexFooter extends PureComponent {

  //
  // Render

  render() {
    const { artist } = this.props;
    const count = artist.length;
    let tracks = 0;
    let trackFiles = 0;
    let ended = 0;
    let continuing = 0;
    let monitored = 0;
    let totalFileSize = 0;

    artist.forEach((s) => {
      const { statistics = {} } = s;

      const {
        trackCount = 0,
        trackFileCount = 0,
        sizeOnDisk = 0
      } = statistics;

      tracks += trackCount;
      trackFiles += trackFileCount;

      if (s.status === 'ended') {
        ended++;
      } else {
        continuing++;
      }

      if (s.monitored) {
        monitored++;
      }

      totalFileSize += sizeOnDisk;
    });

    return (
      <ColorImpairedConsumer>
        {(enableColorImpairedMode) => {
          return (
            <div className={styles.footer}>
              <div>
                <div className={styles.legendItem}>
                  <div
                    className={classNames(
                      styles.continuing,
                      enableColorImpairedMode && 'colorImpaired'
                    )}
                  />
                  <div>Continuing (All tracks downloaded)</div>
                </div>

                <div className={styles.legendItem}>
                  <div
                    className={classNames(
                      styles.ended,
                      enableColorImpairedMode && 'colorImpaired'
                    )}
                  />
                  <div>Ended (All tracks downloaded)</div>
                </div>

                <div className={styles.legendItem}>
                  <div
                    className={classNames(
                      styles.missingMonitored,
                      enableColorImpairedMode && 'colorImpaired'
                    )}
                  />
                  <div>Missing Tracks (Artist monitored)</div>
                </div>

                <div className={styles.legendItem}>
                  <div
                    className={classNames(
                      styles.missingUnmonitored,
                      enableColorImpairedMode && 'colorImpaired'
                    )}
                  />
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
        }}
      </ColorImpairedConsumer>
    );
  }
}

ArtistIndexFooter.propTypes = {
  artist: PropTypes.arrayOf(PropTypes.object).isRequired
};

export default ArtistIndexFooter;
