import React from 'react';
import Audiobook from 'Audiobook/Audiobook';
import styles from './AddNewAudiobook.css';

function AddNewAudiobookSearchResult(props: Audiobook) {
  const { title, narrator, durationMinutes } = props;

  const formatDuration = (minutes: number) => {
    if (!minutes) return null;
    const hours = Math.floor(minutes / 60);
    const mins = minutes % 60;
    return hours > 0 ? `${hours}h ${mins}m` : `${mins}m`;
  };

  return (
    <div className={styles.searchResult}>
      <div className={styles.title}>{title}</div>
      <div className={styles.subtitle}>
        {narrator && <span>Narrated by {narrator}</span>}
        {durationMinutes > 0 && (
          <span> - {formatDuration(durationMinutes)}</span>
        )}
      </div>
    </div>
  );
}

export default AddNewAudiobookSearchResult;
