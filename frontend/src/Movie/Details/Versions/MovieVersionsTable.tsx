import React from 'react';
import Column from 'Components/Table/Column';
import Table from 'Components/Table/Table';
import TableBody from 'Components/Table/TableBody';
import translate from 'Utilities/String/translate';
import MovieVersion from './MovieVersion';
import MovieVersionRow from './MovieVersionRow';

const columns: Column[] = [
  {
    name: 'qualityProfileId',
    label: () => translate('QualityProfile'),
    isVisible: true,
  },
  {
    name: 'path',
    label: () => translate('Path'),
    isVisible: true,
  },
  {
    name: 'hasFile',
    label: () => translate('Status'),
    isVisible: true,
  },
  {
    name: 'actions',
    label: '',
    isVisible: true,
  },
];

interface MovieVersionsTableProps {
  versions: MovieVersion[];
  onRemovePress(id: number): void;
}

function MovieVersionsTable({
  versions,
  onRemovePress,
}: MovieVersionsTableProps) {
  return (
    <Table columns={columns}>
      <TableBody>
        {versions.map((version) => (
          <MovieVersionRow
            key={version.id}
            version={version}
            onRemovePress={onRemovePress}
          />
        ))}
      </TableBody>
    </Table>
  );
}

export default MovieVersionsTable;
