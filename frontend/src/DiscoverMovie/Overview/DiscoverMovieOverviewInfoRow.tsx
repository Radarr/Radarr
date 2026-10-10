import React, { ReactNode } from 'react';
import Icon, { IconName } from 'Components/Icon';
import styles from './DiscoverMovieOverviewInfoRow.css';

interface DiscoverMovieOverviewInfoRowProps {
  title?: string;
  iconName?: IconName;
  icon?: ReactNode;
  label: string | null;
}

function DiscoverMovieOverviewInfoRow(
  props: DiscoverMovieOverviewInfoRowProps
) {
  const { title, iconName, icon, label } = props;

  return (
    <div className={styles.infoRow} title={title}>
      {iconName ? (
        <Icon className={styles.icon} name={iconName} size={14} />
      ) : (
        <span className={styles.icon}>{icon}</span>
      )}

      {label}
    </div>
  );
}

export default DiscoverMovieOverviewInfoRow;
