import React from 'react';
import Icon from 'Components/Icon';
import styles from './MovieIndexOverviewInfoRow.css';

interface MovieIndexOverviewInfoRowProps {
  title?: string;
  iconName: object;
  label: string;
}

function MovieIndexOverviewInfoRow(props: MovieIndexOverviewInfoRowProps) {
  const { title, iconName, label } = props;

  return (
    <div className={styles.infoRow} title={title}>
      <Icon className={styles.icon} name={iconName} size={14} />

      {label}
    </div>
  );
}

export default MovieIndexOverviewInfoRow;
