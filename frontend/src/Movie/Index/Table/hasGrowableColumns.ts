import Column from 'Components/Table/Column';

const growableColumns = ['studio', 'qualityProfileId', 'path', 'tags'];

export default function hasGrowableColumns(columns: Column[]) {
  return columns.some((column) => {
    const { name, isVisible } = column;

    return growableColumns.includes(name) && isVisible;
  });
}
