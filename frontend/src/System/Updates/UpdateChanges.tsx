import React from 'react';
import InlineMarkdown from 'Components/Markdown/InlineMarkdown';
import styles from './UpdateChanges.css';

interface UpdateChangesProps {
  title: string;
  changes: string[];
}

function UpdateChanges(props: UpdateChangesProps) {
  const { title, changes } = props;

  if (changes.length === 0) {
    return null;
  }

  const uniqueChanges = [...new Set(changes)];

  return (
    <div>
      <div className={styles.title}>{title}</div>
      <ul>
        {uniqueChanges.map((change, index) => {
          const checkChange = change.replace(
            /#\d{4,5}\b/g,
            (match) =>
              `[${match}](https://github.com/Radarr/Radarr/issues/${match.substring(
                1
              )})`
          );

          return (
            <li key={index}>
              <InlineMarkdown data={checkChange} />
            </li>
          );
        })}
      </ul>
    </div>
  );
}

export default UpdateChanges;
