import React from 'react';
import { useSelector } from 'react-redux';
import { CommandBody } from 'Commands/Command';
import TableRowCell from 'Components/Table/Cells/TableRowCell';
import createMultiMoviesSelector from 'Store/Selectors/createMultiMoviesSelector';
import translate from 'Utilities/String/translate';
import styles from './QueuedTaskRowNameCell.css';

function formatTitles(titles: string[]) {
  if (!titles) {
    return null;
  }

  if (titles.length > 11) {
    return (
      <span title={titles.join(', ')}>
        {titles.slice(0, 10).join(', ')}, {titles.length - 10} more
      </span>
    );
  }

  return <span>{titles.join(', ')}</span>;
}

export interface QueuedTaskRowNameCellProps {
  commandName: string;
  body: CommandBody;
  clientUserAgent?: string;
}

export default function QueuedTaskRowNameCell(
  props: QueuedTaskRowNameCellProps
) {
  const { commandName, body, clientUserAgent } = props;
  const movieIds = [...(body.movieIds ?? [])];

  if (body.movieId) {
    movieIds.push(body.movieId);
  }

  const movies = useSelector(createMultiMoviesSelector(movieIds));
  const sortedMovies = movies.sort((a, b) =>
    a.sortTitle.localeCompare(b.sortTitle)
  );

  return (
    <TableRowCell>
      <span className={styles.commandName}>
        {commandName}
        {sortedMovies.length ? (
          <span> - {formatTitles(sortedMovies.map((m) => m.title))}</span>
        ) : null}
      </span>

      {clientUserAgent ? (
        <span
          className={styles.userAgent}
          title={translate('TaskUserAgentTooltip')}
        >
          {translate('From')}: {clientUserAgent}
        </span>
      ) : null}
    </TableRowCell>
  );
}
