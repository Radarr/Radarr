import React from 'react';
import { useSelector } from 'react-redux';
import AppState from 'App/State/AppState';
import Icon from 'Components/Icon';
import PageContent from 'Components/Page/PageContent';
import PageContentBody from 'Components/Page/PageContentBody';
import { icons } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import styles from './BookDetails.css';

interface BookDetailsProps {
  bookId: number;
}

function BookDetails({ bookId }: BookDetailsProps) {
  const book = useSelector((state: AppState) =>
    state.books.items.find((b) => b.id === bookId)
  );

  if (!book) {
    return null;
  }

  const {
    title,
    description,
    isbn,
    isbn13,
    asin,
    pageCount,
    releaseDate,
    publisher,
    language,
    monitored,
  } = book;

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

            {pageCount > 0 && (
              <div className={styles.detailRow}>
                <span className={styles.label}>{translate('PageCount')}:</span>
                <span className={styles.value}>{pageCount}</span>
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

            {isbn13 && (
              <div className={styles.detailRow}>
                <span className={styles.label}>ISBN-13:</span>
                <span className={styles.value}>{isbn13}</span>
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

export default BookDetails;
