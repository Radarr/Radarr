import React, { useCallback } from 'react';
import { useSelector } from 'react-redux';
import MovieTagList from 'Components/MovieTagList';
import TableRowCell from 'Components/Table/Cells/TableRowCell';
import TableSelectCell from 'Components/Table/Cells/TableSelectCell';
import Column from 'Components/Table/Column';
import TableRow from 'Components/Table/TableRow';
import { createQualityProfileSelectorForHook } from 'Store/Selectors/createQualityProfileSelector';
import { SelectStateInputProps } from 'typings/props';
import firstCharToUpper from 'Utilities/String/firstCharToUpper';
import translate from 'Utilities/String/translate';
import styles from './ManageImportListsModalRow.css';

interface ManageImportListsModalRowProps {
  id: number;
  name: string;
  rootFolderPath: string;
  qualityProfileId: number;
  minimumAvailability: string;
  implementation: string;
  tags: number[];
  retroApplyTags: boolean;
  enabled: boolean;
  enableAuto: boolean;
  columns: Column[];
  isSelected?: boolean;
  onSelectedChange(result: SelectStateInputProps): void;
}

function ManageImportListsModalRow(props: ManageImportListsModalRowProps) {
  const {
    id,
    isSelected,
    name,
    rootFolderPath,
    minimumAvailability,
    qualityProfileId,
    implementation,
    enabled,
    enableAuto,
    tags,
    retroApplyTags,
    onSelectedChange,
  } = props;

  const qualityProfile = useSelector(
    createQualityProfileSelectorForHook(qualityProfileId)
  );

  const onSelectedChangeWrapper = useCallback(
    (result: SelectStateInputProps) => {
      onSelectedChange({
        ...result,
      });
    },
    [onSelectedChange]
  );

  return (
    <TableRow>
      <TableSelectCell
        id={id}
        isSelected={isSelected}
        onSelectedChange={onSelectedChangeWrapper}
      />

      <TableRowCell className={styles.name}>{name}</TableRowCell>

      <TableRowCell className={styles.implementation}>
        {implementation}
      </TableRowCell>

      <TableRowCell className={styles.qualityProfileId}>
        {qualityProfile?.name ?? translate('None')}
      </TableRowCell>

      <TableRowCell className={styles.minimumAvailability}>
        {translate(firstCharToUpper(minimumAvailability))}
      </TableRowCell>

      <TableRowCell className={styles.rootFolderPath}>
        {rootFolderPath}
      </TableRowCell>

      <TableRowCell className={styles.enabled}>
        {enabled ? translate('Yes') : translate('No')}
      </TableRowCell>

      <TableRowCell className={styles.enableAuto}>
        {enableAuto ? translate('Yes') : translate('No')}
      </TableRowCell>

      <TableRowCell className={styles.tags}>
        <MovieTagList tags={tags} />
      </TableRowCell>

      <TableRowCell className={styles.enabled}>
        {retroApplyTags ? translate('Yes') : translate('No')}
      </TableRowCell>
    </TableRow>
  );
}

export default ManageImportListsModalRow;
