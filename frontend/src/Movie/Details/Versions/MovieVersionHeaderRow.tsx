import React, { useCallback } from 'react';
import Label from 'Components/Label';
import IconButton from 'Components/Link/IconButton';
import TableRowCell from 'Components/Table/Cells/TableRowCell';
import TableRow from 'Components/Table/TableRow';
import { icons, kinds } from 'Helpers/Props';
import QualityProfileName from 'Settings/Profiles/Quality/QualityProfileName';
import translate from 'Utilities/String/translate';
import MovieVersion from './MovieVersion';
import styles from './MovieVersionHeaderRow.css';

interface MovieVersionHeaderRowProps {
  version: MovieVersion;
  colSpan: number;
  onRemovePress(id: number): void;
}

function MovieVersionHeaderRow({
  version,
  colSpan,
  onRemovePress,
}: MovieVersionHeaderRowProps) {
  const { id, qualityProfileId, path, hasFile, isPrimary } = version;

  const handleRemovePress = useCallback(() => {
    onRemovePress(id);
  }, [id, onRemovePress]);

  return (
    <TableRow className={styles.row}>
      <TableRowCell className={styles.cell} colSpan={colSpan}>
        <div className={styles.header}>
          <span className={styles.profile}>
            <QualityProfileName qualityProfileId={qualityProfileId} />
          </span>

          <span className={styles.path}>{path}</span>

          {isPrimary ? (
            <Label kind={kinds.INFO}>{translate('PrimaryVersion')}</Label>
          ) : null}

          {hasFile ? null : (
            <Label kind={kinds.DANGER}>{translate('Missing')}</Label>
          )}

          {isPrimary || hasFile ? null : (
            <IconButton
              className={styles.remove}
              name={icons.REMOVE}
              title={translate('RemoveVersion')}
              onPress={handleRemovePress}
            />
          )}
        </div>
      </TableRowCell>
    </TableRow>
  );
}

export default MovieVersionHeaderRow;
