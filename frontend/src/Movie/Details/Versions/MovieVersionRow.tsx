import React, { useCallback } from 'react';
import Label from 'Components/Label';
import IconButton from 'Components/Link/IconButton';
import TableRowCell from 'Components/Table/Cells/TableRowCell';
import TableRow from 'Components/Table/TableRow';
import { icons, kinds } from 'Helpers/Props';
import QualityProfileName from 'Settings/Profiles/Quality/QualityProfileName';
import translate from 'Utilities/String/translate';
import MovieVersion from './MovieVersion';
import styles from './MovieVersionRow.css';

interface MovieVersionRowProps {
  version: MovieVersion;
  onRemovePress(id: number): void;
}

function MovieVersionRow({ version, onRemovePress }: MovieVersionRowProps) {
  const { id, qualityProfileId, path, hasFile, isPrimary } = version;

  const handleRemovePress = useCallback(() => {
    onRemovePress(id);
  }, [id, onRemovePress]);

  return (
    <TableRow>
      <TableRowCell>
        <QualityProfileName qualityProfileId={qualityProfileId} />

        {isPrimary ? (
          <Label className={styles.primary} kind={kinds.INFO}>
            {translate('PrimaryVersion')}
          </Label>
        ) : null}
      </TableRowCell>

      <TableRowCell>{path}</TableRowCell>

      <TableRowCell>
        <Label kind={hasFile ? kinds.SUCCESS : kinds.DANGER}>
          {hasFile ? translate('Downloaded') : translate('Missing')}
        </Label>
      </TableRowCell>

      <TableRowCell className={styles.actions}>
        {isPrimary || hasFile ? null : (
          <IconButton
            name={icons.REMOVE}
            title={translate('RemoveVersion')}
            onPress={handleRemovePress}
          />
        )}
      </TableRowCell>
    </TableRow>
  );
}

export default MovieVersionRow;
