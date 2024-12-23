import React from 'react';
import Label from 'Components/Label';
import VirtualTableRowCell from 'Components/Table/Cells/VirtualTableRowCell';
import styles from './SelectMovieRow.css';

interface SelectMovieRowProps {
  title: string;
  tmdbId: number;
  imdbId?: string;
  year: number;
}

function SelectMovieRow({ title, year, tmdbId, imdbId }: SelectMovieRowProps) {
  return (
    <>
      <VirtualTableRowCell className={styles.title}>
        {title}
      </VirtualTableRowCell>

      <VirtualTableRowCell className={styles.year}>{year}</VirtualTableRowCell>

      <VirtualTableRowCell className={styles.imdbId}>
        {imdbId ? <Label>{imdbId}</Label> : null}
      </VirtualTableRowCell>

      <VirtualTableRowCell className={styles.tmdbId}>
        <Label>{tmdbId}</Label>
      </VirtualTableRowCell>
    </>
  );
}

export default SelectMovieRow;
