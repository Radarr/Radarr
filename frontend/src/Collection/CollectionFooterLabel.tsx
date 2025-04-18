import React from 'react';
import SpinnerIcon from 'Components/SpinnerIcon';
import { icons } from 'Helpers/Props';
import styles from './CollectionFooterLabel.css';

interface CollectionFooterLabelProps {
  className?: string;
  label: string;
  isSaving: boolean;
}

function CollectionFooterLabel({
  className = styles.label,
  label,
  isSaving,
}: CollectionFooterLabelProps) {
  return (
    <div className={className}>
      {label}

      {isSaving ? (
        <SpinnerIcon
          className={styles.savingIcon}
          name={icons.SPINNER}
          isSpinning={true}
        />
      ) : null}
    </div>
  );
}

export default CollectionFooterLabel;
