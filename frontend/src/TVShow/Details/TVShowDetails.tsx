import React from 'react';
import { useSelector } from 'react-redux';
import AppState from 'App/State/AppState';
import Icon from 'Components/Icon';
import PageContent from 'Components/Page/PageContent';
import PageContentBody from 'Components/Page/PageContentBody';
import { icons } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import styles from './TVShowDetails.css';

interface TVShowDetailsProps {
  readonly tvShowId: number;
}

function TVShowDetails({ tvShowId }: Readonly<TVShowDetailsProps>) {
  const tvShow = useSelector((state: AppState) =>
    state.tvShows.items.find((s) => s.id === tvShowId)
  );

  if (!tvShow) {
    return null;
  }

  const {
    title,
    year,
    network,
    status,
    overview,
    monitored,
    seriesType,
    isAnime,
    genres,
    runtime,
    certification,
    firstAired,
  } = tvShow;

  return (
    <PageContent title={title}>
      <PageContentBody>
        <div className={styles.container}>
          <div className={styles.header}>
            <h1 className={styles.title}>
              {title} ({year})
              <Icon
                className={styles.monitoredIcon}
                name={monitored ? icons.MONITORED : icons.UNMONITORED}
                title={monitored ? 'Monitored' : 'Unmonitored'}
                size={24}
              />
            </h1>
          </div>

          <div className={styles.details}>
            {network && (
              <div className={styles.detailRow}>
                <span className={styles.label}>{translate('Network')}:</span>
                <span className={styles.value}>{network}</span>
              </div>
            )}

            <div className={styles.detailRow}>
              <span className={styles.label}>{translate('Status')}:</span>
              <span className={styles.value}>{status}</span>
            </div>

            <div className={styles.detailRow}>
              <span className={styles.label}>{translate('SeriesType')}:</span>
              <span className={styles.value}>
                {seriesType} {isAnime && '(Anime)'}
              </span>
            </div>

            {runtime && (
              <div className={styles.detailRow}>
                <span className={styles.label}>{translate('Runtime')}:</span>
                <span className={styles.value}>{runtime} min</span>
              </div>
            )}

            {certification && (
              <div className={styles.detailRow}>
                <span className={styles.label}>
                  {translate('Certification')}:
                </span>
                <span className={styles.value}>{certification}</span>
              </div>
            )}

            {firstAired && (
              <div className={styles.detailRow}>
                <span className={styles.label}>{translate('FirstAired')}:</span>
                <span className={styles.value}>{firstAired}</span>
              </div>
            )}

            {genres && genres.length > 0 && (
              <div className={styles.detailRow}>
                <span className={styles.label}>{translate('Genres')}:</span>
                <div className={styles.genres}>
                  {genres.map((genre) => (
                    <span key={genre} className={styles.genre}>
                      {genre}
                    </span>
                  ))}
                </div>
              </div>
            )}
          </div>

          {overview && (
            <div className={styles.overview}>
              <p>{overview}</p>
            </div>
          )}
        </div>
      </PageContentBody>
    </PageContent>
  );
}

export default TVShowDetails;
