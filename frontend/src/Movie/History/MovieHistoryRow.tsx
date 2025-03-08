import React, { useCallback, useState } from 'react';
import HistoryDetailsModal from 'Activity/History/Details/HistoryDetailsModal';
import HistoryEventTypeCell from 'Activity/History/HistoryEventTypeCell';
import IconButton from 'Components/Link/IconButton';
import ConfirmModal from 'Components/Modal/ConfirmModal';
import RelativeDateCell from 'Components/Table/Cells/RelativeDateCell';
import TableRowCell from 'Components/Table/Cells/TableRowCell';
import TableRow from 'Components/Table/TableRow';
import { icons, kinds } from 'Helpers/Props';
import Language from 'Language/Language';
import MovieFormats from 'Movie/MovieFormats';
import MovieLanguages from 'Movie/MovieLanguages';
import MovieQuality from 'Movie/MovieQuality';
import { QualityModel } from 'Quality/Quality';
import CustomFormat from 'typings/CustomFormat';
import { HistoryData, HistoryEventType } from 'typings/History';
import formatCustomFormatScore from 'Utilities/Number/formatCustomFormatScore';
import translate from 'Utilities/String/translate';
import styles from './MovieHistoryRow.css';

interface MovieHistoryRowProps {
  id: number;
  eventType: HistoryEventType;
  sourceTitle: string;
  languages?: Language[];
  quality: QualityModel;
  qualityCutoffNotMet: boolean;
  customFormats?: CustomFormat[];
  customFormatScore: number;
  date: string;
  data: HistoryData;
  downloadId?: string;
  onMarkAsFailedPress: (historyId: number) => void;
}

function MovieHistoryRow({
  id,
  eventType,
  sourceTitle,
  languages = [],
  quality,
  qualityCutoffNotMet,
  customFormats = [],
  customFormatScore,
  date,
  data,
  downloadId,
  onMarkAsFailedPress,
}: MovieHistoryRowProps) {
  const [isDetailsModalOpen, setIsDetailsModalOpen] = useState(false);
  const [isMarkAsFailedModalOpen, setIsMarkAsFailedModalOpen] = useState(false);

  const handleDetailsPress = useCallback(() => {
    setIsDetailsModalOpen(true);
  }, [setIsDetailsModalOpen]);

  const handleDetailsModalClose = useCallback(() => {
    setIsDetailsModalOpen(false);
  }, [setIsDetailsModalOpen]);

  const handleMarkAsFailedPress = useCallback(() => {
    setIsMarkAsFailedModalOpen(true);
  }, []);

  const handleConfirmMarkAsFailed = useCallback(() => {
    onMarkAsFailedPress(id);
    setIsMarkAsFailedModalOpen(false);
  }, [id, onMarkAsFailedPress]);

  const handleMarkAsFailedModalClose = useCallback(() => {
    setIsMarkAsFailedModalOpen(false);
  }, []);

  return (
    <TableRow>
      <HistoryEventTypeCell eventType={eventType} data={data} />

      <TableRowCell className={styles.sourceTitle}>{sourceTitle}</TableRowCell>

      <TableRowCell>
        <MovieLanguages languages={languages} />
      </TableRowCell>

      <TableRowCell>
        <MovieQuality quality={quality} isCutoffNotMet={qualityCutoffNotMet} />
      </TableRowCell>

      <TableRowCell>
        <MovieFormats formats={customFormats} />
      </TableRowCell>

      <TableRowCell className={styles.customFormatScore}>
        {formatCustomFormatScore(customFormatScore, customFormats.length)}
      </TableRowCell>

      <RelativeDateCell date={date} includeSeconds={true} includeTime={true} />

      <TableRowCell className={styles.actions}>
        <IconButton name={icons.INFO} onPress={handleDetailsPress} />

        {eventType === 'grabbed' ? (
          <IconButton
            title={translate('MarkAsFailed')}
            name={icons.REMOVE}
            size={14}
            onPress={handleMarkAsFailedPress}
          />
        ) : null}
      </TableRowCell>

      <ConfirmModal
        isOpen={isMarkAsFailedModalOpen}
        kind={kinds.DANGER}
        title={translate('MarkAsFailed')}
        message={translate('MarkAsFailedConfirmation', { sourceTitle })}
        confirmLabel={translate('MarkAsFailed')}
        onConfirm={handleConfirmMarkAsFailed}
        onCancel={handleMarkAsFailedModalClose}
      />

      <HistoryDetailsModal
        isOpen={isDetailsModalOpen}
        eventType={eventType}
        sourceTitle={sourceTitle}
        data={data}
        downloadId={downloadId}
        onMarkAsFailedPress={handleMarkAsFailedPress}
        onModalClose={handleDetailsModalClose}
      />
    </TableRow>
  );
}

export default MovieHistoryRow;
