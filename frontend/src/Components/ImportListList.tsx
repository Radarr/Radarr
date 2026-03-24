import React from 'react';
import { useSelector } from 'react-redux';
import AppState from 'App/State/AppState';
import Label from './Label';
import styles from './ImportListList.css';

interface ImportListListProps {
  lists: number[];
}

function ImportListList({ lists }: ImportListListProps) {
  const allImportLists = useSelector(
    (state: AppState) => state.settings.importLists.items
  );

  return (
    <div className={styles.lists}>
      {lists.map((id) => {
        const importList = allImportLists.find((list) => list.id === id);

        if (!importList) {
          return null;
        }

        return (
          <Label key={importList.id} kind="success" size="medium">
            {importList.name}
          </Label>
        );
      })}
    </div>
  );
}

export default ImportListList;
