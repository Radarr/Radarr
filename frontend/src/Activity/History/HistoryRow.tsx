import React, { useCallback, useEffect, useState } from 'react';
import { useDispatch } from 'react-redux';
import IconButton from 'Components/Link/IconButton';
import RelativeDateCell from 'Components/Table/Cells/RelativeDateCell';
import TableRowCell from 'Components/Table/Cells/TableRowCell';
import Column from 'Components/Table/Column';
import TableRow from 'Components/Table/TableRow';
import Tooltip from 'Components/Tooltip/Tooltip';
import usePrevious from 'Helpers/Hooks/usePrevious';
import { icons, tooltipPositions } from 'Helpers/Props';
import Language from 'Language/Language';
import MovieFormats from 'Movie/MovieFormats';
import MovieLanguages from 'Movie/MovieLanguages';
import MovieQuality from 'Movie/MovieQuality';
import MovieTitleLink from 'Movie/MovieTitleLink';
import useMovie from 'Movie/useMovie';
import { QualityModel } from 'Quality/Quality';
import { fetchHistory, markAsFailed } from 'Store/Actions/historyActions';
import CustomFormat from 'typings/CustomFormat';
import { HistoryData, HistoryEventType } from 'typings/History';
import formatCustomFormatScore from 'Utilities/Number/formatCustomFormatScore';
import HistoryDetailsModal from './Details/HistoryDetailsModal';
import HistoryEventTypeCell from './HistoryEventTypeCell';
import styles from './HistoryRow.css';

interface HistoryRowProps {
  id: number;
  movieId: number;
  languages: Language[];
  quality: QualityModel;
  customFormats?: CustomFormat[];
  customFormatScore: number;
  qualityCutoffNotMet: boolean;
  eventType: HistoryEventType;
  sourceTitle: string;
  date: string;
  data: HistoryData;
  downloadId?: string;
  isMarkingAsFailed?: boolean;
  markAsFailedError?: object;
  columns: Column[];
}

function HistoryRow(props: HistoryRowProps) {
  const {
    id,
    movieId,
    languages,
    quality,
    customFormats = [],
    customFormatScore,
    qualityCutoffNotMet,
    eventType,
    sourceTitle,
    date,
    data,
    downloadId,
    isMarkingAsFailed = false,
    markAsFailedError,
    columns,
  } = props;

  const wasMarkingAsFailed = usePrevious(isMarkingAsFailed);
  const dispatch = useDispatch();
  const movie = useMovie(movieId);

  const [isDetailsModalOpen, setIsDetailsModalOpen] = useState(false);

  const handleDetailsPress = useCallback(() => {
    setIsDetailsModalOpen(true);
  }, [setIsDetailsModalOpen]);

  const handleDetailsModalClose = useCallback(() => {
    setIsDetailsModalOpen(false);
  }, [setIsDetailsModalOpen]);

  const handleMarkAsFailedPress = useCallback(() => {
    dispatch(markAsFailed({ id }));
  }, [id, dispatch]);

  useEffect(() => {
    if (wasMarkingAsFailed && !isMarkingAsFailed && !markAsFailedError) {
      setIsDetailsModalOpen(false);
      dispatch(fetchHistory());
    }
  }, [
    wasMarkingAsFailed,
    isMarkingAsFailed,
    markAsFailedError,
    setIsDetailsModalOpen,
    dispatch,
  ]);

  if (!movie) {
    return null;
  }

  return (
    <TableRow>
      {columns.map((column) => {
        const { name, isVisible } = column;

        if (!isVisible) {
          return null;
        }

        if (name === 'eventType') {
          return (
            <HistoryEventTypeCell
              key={name}
              eventType={eventType}
              data={data}
            />
          );
        }

        if (name === 'movieMetadata.sortTitle') {
          return (
            <TableRowCell key={name}>
              <MovieTitleLink titleSlug={movie.titleSlug} title={movie.title} />
            </TableRowCell>
          );
        }

        if (name === 'languages') {
          return (
            <TableRowCell key={name}>
              <MovieLanguages languages={languages} />
            </TableRowCell>
          );
        }

        if (name === 'quality') {
          return (
            <TableRowCell key={name}>
              <MovieQuality
                quality={quality}
                isCutoffNotMet={qualityCutoffNotMet}
              />
            </TableRowCell>
          );
        }

        if (name === 'customFormats') {
          return (
            <TableRowCell key={name}>
              <MovieFormats formats={customFormats} />
            </TableRowCell>
          );
        }

        if (name === 'date') {
          return <RelativeDateCell key={name} date={date} />;
        }

        if (name === 'downloadClient') {
          return (
            <TableRowCell key={name} className={styles.downloadClient}>
              {'downloadClient' in data ? data.downloadClient : ''}
            </TableRowCell>
          );
        }

        if (name === 'indexer') {
          return (
            <TableRowCell key={name} className={styles.indexer}>
              {'indexer' in data ? data.indexer : ''}
            </TableRowCell>
          );
        }

        if (name === 'customFormatScore') {
          return (
            <TableRowCell key={name} className={styles.customFormatScore}>
              <Tooltip
                anchor={formatCustomFormatScore(
                  customFormatScore,
                  customFormats.length
                )}
                tooltip={<MovieFormats formats={customFormats} />}
                position={tooltipPositions.BOTTOM}
              />
            </TableRowCell>
          );
        }

        if (name === 'releaseGroup') {
          return (
            <TableRowCell key={name} className={styles.releaseGroup}>
              {'releaseGroup' in data ? data.releaseGroup : ''}
            </TableRowCell>
          );
        }

        if (name === 'sourceTitle') {
          return <TableRowCell key={name}>{sourceTitle}</TableRowCell>;
        }

        if (name === 'details') {
          return (
            <TableRowCell key={name} className={styles.details}>
              <IconButton name={icons.INFO} onPress={handleDetailsPress} />
            </TableRowCell>
          );
        }

        return null;
      })}

      <HistoryDetailsModal
        isOpen={isDetailsModalOpen}
        eventType={eventType}
        sourceTitle={sourceTitle}
        data={data}
        downloadId={downloadId}
        isMarkingAsFailed={isMarkingAsFailed}
        onMarkAsFailedPress={handleMarkAsFailedPress}
        onModalClose={handleDetailsModalClose}
      />
    </TableRow>
  );
}

export default HistoryRow;
