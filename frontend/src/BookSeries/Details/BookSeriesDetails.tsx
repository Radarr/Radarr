import React from 'react';
import { useSelector } from 'react-redux';
import AppState from 'App/State/AppState';
import Icon from 'Components/Icon';
import PageContent from 'Components/Page/PageContent';
import PageContentBody from 'Components/Page/PageContentBody';
import { icons } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import styles from './BookSeriesDetails.css';

interface BookSeriesDetailsProps {
  readonly bookSeriesId: number;
}

function BookSeriesDetails({ bookSeriesId }: Readonly<BookSeriesDetailsProps>) {
  const bookSeries = useSelector((state: AppState) =>
    state.bookSeries.items.find((s) => s.id === bookSeriesId)
  );

  if (!bookSeries) {
    return null;
  }

  const { title, sortTitle, description, monitored, authorId } = bookSeries;

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
            {sortTitle && sortTitle !== title && (
              <div className={styles.detailRow}>
                <span className={styles.label}>{translate('SortTitle')}:</span>
                <span className={styles.value}>{sortTitle}</span>
              </div>
            )}

            {authorId && (
              <div className={styles.detailRow}>
                <span className={styles.label}>{translate('Author')}:</span>
                <span className={styles.value}>ID: {authorId}</span>
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

export default BookSeriesDetails;
