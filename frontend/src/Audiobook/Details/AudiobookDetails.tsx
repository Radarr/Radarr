import React from 'react';
import { useSelector } from 'react-redux';
import AppState from 'App/State/AppState';
import Icon from 'Components/Icon';
import PageContent from 'Components/Page/PageContent';
import PageContentBody from 'Components/Page/PageContentBody';
import { icons } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import styles from './AudiobookDetails.css';

interface AudiobookDetailsProps {
  readonly audiobookId: number;
}

function formatDuration(minutes: number): string {
  if (!minutes) return '';
  const hours = Math.floor(minutes / 60);
  const mins = minutes % 60;
  return hours > 0 ? `${hours}h ${mins}m` : `${mins}m`;
}

function AudiobookDetails({ audiobookId }: Readonly<AudiobookDetailsProps>) {
  const audiobook = useSelector((state: AppState) =>
    state.audiobooks.items.find((a) => a.id === audiobookId)
  );

  if (!audiobook) {
    return null;
  }

  const {
    title,
    description,
    narrator,
    durationMinutes,
    isbn,
    asin,
    releaseDate,
    publisher,
    language,
    monitored,
  } = audiobook;

  return (
    <PageContent title={title}>
      <PageContentBody>
        <div className={styles.container}>
          <div className={styles.header}>
            <h1 className={styles.title}>
              {title}
              <Icon
                className={styles.monitoredIcon}
                name={monitored ? icons.MONITORED : icons.UNMONITORED}
                title={monitored ? 'Monitored' : 'Unmonitored'}
                size={24}
              />
            </h1>
          </div>

          <div className={styles.details}>
            {narrator && (
              <div className={styles.detailRow}>
                <span className={styles.label}>{translate('Narrator')}:</span>
                <span className={styles.value}>{narrator}</span>
              </div>
            )}

            {durationMinutes > 0 && (
              <div className={styles.detailRow}>
                <span className={styles.label}>{translate('Duration')}:</span>
                <span className={styles.value}>
                  {formatDuration(durationMinutes)}
                </span>
              </div>
            )}

            {publisher && (
              <div className={styles.detailRow}>
                <span className={styles.label}>{translate('Publisher')}:</span>
                <span className={styles.value}>{publisher}</span>
              </div>
            )}

            {releaseDate && (
              <div className={styles.detailRow}>
                <span className={styles.label}>
                  {translate('ReleaseDate')}:
                </span>
                <span className={styles.value}>
                  {new Date(releaseDate).toLocaleDateString()}
                </span>
              </div>
            )}

            {language && (
              <div className={styles.detailRow}>
                <span className={styles.label}>{translate('Language')}:</span>
                <span className={styles.value}>{language}</span>
              </div>
            )}

            {isbn && (
              <div className={styles.detailRow}>
                <span className={styles.label}>ISBN:</span>
                <span className={styles.value}>{isbn}</span>
              </div>
            )}

            {asin && (
              <div className={styles.detailRow}>
                <span className={styles.label}>ASIN:</span>
                <span className={styles.value}>{asin}</span>
              </div>
            )}
          </div>

          {description && (
            <div className={styles.description}>
              <p>{description}</p>
            </div>
          )}
        </div>
      </PageContentBody>
    </PageContent>
  );
}

export default AudiobookDetails;
