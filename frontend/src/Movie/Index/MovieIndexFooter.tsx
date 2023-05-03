import classNames from 'classnames';
import React from 'react';
import { useSelector } from 'react-redux';
import { createSelector } from 'reselect';
import { ColorImpairedConsumer } from 'App/ColorImpairedContext';
import MoviesAppState from 'App/State/MoviesAppState';
import DescriptionList from 'Components/DescriptionList/DescriptionList';
import DescriptionListItem from 'Components/DescriptionList/DescriptionListItem';
import createClientSideCollectionSelector from 'Store/Selectors/createClientSideCollectionSelector';
import createDeepEqualSelector from 'Store/Selectors/createDeepEqualSelector';
import formatBytes from 'Utilities/Number/formatBytes';
import translate from 'Utilities/String/translate';
import styles from './MovieIndexFooter.css';

function createUnoptimizedSelector() {
  return createSelector(
    createClientSideCollectionSelector('movies', 'movieIndex'),
    (movies: MoviesAppState) => {
      return movies.items.map((m) => {
        const { monitored, status, hasFile, sizeOnDisk } = m;

        return {
          monitored,
          status,
          hasFile,
          sizeOnDisk,
        };
      });
    }
  );
}

function createMovieSelector() {
  return createDeepEqualSelector(
    createUnoptimizedSelector(),
    (movies) => movies
  );
}

export default function MovieIndexFooter() {
  const movies = useSelector(createMovieSelector());
  const count = movies.length;
  let movieFiles = 0;
  let monitored = 0;
  let totalFileSize = 0;

  movies.forEach((s) => {
    if (s.hasFile) {
      movieFiles += 1;
    }

    if (s.monitored) {
      monitored++;
    }

    totalFileSize += s.sizeOnDisk;
  });

  return (
    <ColorImpairedConsumer>
      {(enableColorImpairedMode) => {
        return (
          <div className={styles.footer}>
            <div>
              <div className={styles.legendItem}>
                <div className={styles.ended} />
                <div>{translate('DownloadedAndMonitored')}</div>
              </div>

              <div className={styles.legendItem}>
                <div className={styles.availNotMonitored} />
                <div>{translate('DownloadedButNotMonitored')}</div>
              </div>

              <div className={styles.legendItem}>
                <div
                  className={classNames(
                    styles.missingMonitored,
                    enableColorImpairedMode && 'colorImpaired'
                  )}
                />
                <div>{translate('MissingMonitoredAndConsideredAvailable')}</div>
              </div>

              <div className={styles.legendItem}>
                <div
                  className={classNames(
                    styles.missingUnmonitored,
                    enableColorImpairedMode && 'colorImpaired'
                  )}
                />
                <div>{translate('MissingNotMonitored')}</div>
              </div>

              <div className={styles.legendItem}>
                <div className={styles.queue} />
                <div>{translate('Queued')}</div>
              </div>

              <div className={styles.legendItem}>
                <div className={styles.continuing} />
                <div>{translate('Unreleased')}</div>
              </div>
            </div>

            <div className={styles.statistics}>
              <DescriptionList>
                <DescriptionListItem title={translate('Movies')} data={count} />

                <DescriptionListItem
                  title={translate('MovieFiles')}
                  data={movieFiles}
                />
              </DescriptionList>

              <DescriptionList>
                <DescriptionListItem
                  title={translate('Monitored')}
                  data={monitored}
                />

                <DescriptionListItem
                  title={translate('Unmonitored')}
                  data={count - monitored}
                />
              </DescriptionList>

              <DescriptionList>
                <DescriptionListItem
                  title={translate('TotalFileSize')}
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
