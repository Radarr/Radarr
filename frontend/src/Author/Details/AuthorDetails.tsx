import React from 'react';
import { useSelector } from 'react-redux';
import AppState from 'App/State/AppState';
import Icon from 'Components/Icon';
import PageContent from 'Components/Page/PageContent';
import PageContentBody from 'Components/Page/PageContentBody';
import { icons } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import styles from './AuthorDetails.css';

interface AuthorDetailsProps {
  readonly authorId: number;
}

function AuthorDetails({ authorId }: Readonly<AuthorDetailsProps>) {
  const author = useSelector((state: AppState) =>
    state.authors.items.find((a) => a.id === authorId)
  );

  if (!author) {
    return null;
  }

  const { name, sortName, description, path, monitored, added } = author;

  return (
    <PageContent title={name}>
      <PageContentBody>
        <div className={styles.container}>
          <div className={styles.header}>
            <h1 className={styles.title}>
              {name}
              <Icon
                className={styles.monitoredIcon}
                name={monitored ? icons.MONITORED : icons.UNMONITORED}
                title={monitored ? 'Monitored' : 'Unmonitored'}
                size={24}
              />
            </h1>
          </div>

          <div className={styles.details}>
            {sortName && sortName !== name && (
              <div className={styles.detailRow}>
                <span className={styles.label}>{translate('SortName')}:</span>
                <span className={styles.value}>{sortName}</span>
              </div>
            )}

            {path && (
              <div className={styles.detailRow}>
                <span className={styles.label}>{translate('Path')}:</span>
                <span className={styles.value}>{path}</span>
              </div>
            )}

            {added && (
              <div className={styles.detailRow}>
                <span className={styles.label}>{translate('Added')}:</span>
                <span className={styles.value}>
                  {new Date(added).toLocaleDateString()}
                </span>
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

export default AuthorDetails;
