import React, { useCallback, useEffect } from 'react';
import CheckInput from 'Components/Form/CheckInput';
import Icon from 'Components/Icon';
import { icons, kinds } from 'Helpers/Props';
import { CheckInputChanged } from 'typings/inputs';
import { SelectStateInputProps } from 'typings/props';
import translate from 'Utilities/String/translate';
import styles from './OrganizePreviewRow.css';

interface OrganizePreviewRowProps {
  id: number;
  isMovieFolder?: boolean;
  existingPath: string;
  newPath: string;
  isSelected?: boolean;
  onSelectedChange: (props: SelectStateInputProps) => void;
}

function OrganizePreviewRow({
  id,
  isMovieFolder,
  existingPath,
  newPath,
  isSelected,
  onSelectedChange,
}: OrganizePreviewRowProps) {
  const handleSelectedChange = useCallback(
    ({ value, shiftKey }: CheckInputChanged) => {
      onSelectedChange({ id, value, shiftKey });
    },
    [id, onSelectedChange]
  );

  useEffect(() => {
    onSelectedChange({ id, value: true, shiftKey: false });
  }, [id, onSelectedChange]);

  return (
    <div className={isMovieFolder ? styles.movieFolderRow : styles.row}>
      <CheckInput
        containerClassName={styles.selectedContainer}
        name={id.toString()}
        value={isSelected}
        onChange={handleSelectedChange}
      />

      <div>
        {isMovieFolder ? (
          <div className={styles.movieFolderLabel}>
            <Icon name={icons.FOLDER} />

            <span className={styles.path}>{translate('MovieFolder')}</span>
          </div>
        ) : null}

        <div>
          <Icon name={icons.SUBTRACT} kind={kinds.DANGER} />

          <span className={styles.path}>{existingPath}</span>
        </div>

        <div>
          <Icon name={icons.ADD} kind={kinds.SUCCESS} />

          <span className={styles.path}>{newPath}</span>
        </div>
      </div>
    </div>
  );
}

export default OrganizePreviewRow;
