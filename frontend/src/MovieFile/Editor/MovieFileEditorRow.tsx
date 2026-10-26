import React, { useCallback } from 'react';
import Icon from 'Components/Icon';
import IconButton from 'Components/Link/IconButton';
import ConfirmModal from 'Components/Modal/ConfirmModal';
import RelativeDateCell from 'Components/Table/Cells/RelativeDateCell';
import TableRowCell from 'Components/Table/Cells/TableRowCell';
import Column from 'Components/Table/Column';
import TableRow from 'Components/Table/TableRow';
import Popover from 'Components/Tooltip/Popover';
import Tooltip from 'Components/Tooltip/Tooltip';
import useModalOpenState from 'Helpers/Hooks/useModalOpenState';
import { icons, kinds, tooltipPositions } from 'Helpers/Props';
import IndexerFlags from 'Movie/IndexerFlags';
import MovieFormats from 'Movie/MovieFormats';
import MovieLanguages from 'Movie/MovieLanguages';
import MovieQuality from 'Movie/MovieQuality';
import FileEditModal from 'MovieFile/Edit/FileEditModal';
import MediaInfo from 'MovieFile/MediaInfo';
import { MovieFile } from 'MovieFile/MovieFile';
import formatBytes from 'Utilities/Number/formatBytes';
import formatCustomFormatScore from 'Utilities/Number/formatCustomFormatScore';
import translate from 'Utilities/String/translate';
import FileDetailsModal from '../FileDetailsModal';
import MovieFileRowCellPlaceholder from './MovieFileRowCellPlaceholder';
import styles from './MovieFileEditorRow.css';

interface MovieFileRowProps
  extends Pick<
    MovieFile,
    | 'id'
    | 'relativePath'
    | 'size'
    | 'releaseGroup'
    | 'indexerFlags'
    | 'languages'
    | 'quality'
    | 'customFormats'
    | 'customFormatScore'
    | 'qualityCutoffNotMet'
    | 'mediaInfo'
    | 'dateAdded'
  > {
  columns: Column[];
  onDeleteMovieFile(movieFileId: number): void;
}

function MovieFileEditorRow(props: MovieFileRowProps) {
  const {
    id,
    mediaInfo,
    relativePath,
    size,
    releaseGroup,
    quality,
    qualityCutoffNotMet,
    customFormats = [],
    customFormatScore = 0,
    indexerFlags,
    languages,
    dateAdded,
    columns,
    onDeleteMovieFile,
  } = props;

  const [
    isMovieFileDetailsModalOpen,
    onMovieFileDetailsModalPress,
    onMovieFileDetailsModalClose,
  ] = useModalOpenState(false);
  const [
    isMovieFileEditModalOpen,
    onMovieFileEditModalPress,
    onMovieFileEditModalClose,
  ] = useModalOpenState(false);
  const [
    isRemoveMovieFileModalOpen,
    setRemoveMovieFileModalOpen,
    setRemoveMovieFileModalClosed,
  ] = useModalOpenState(false);

  const handleRemoveMovieFilePress = useCallback(() => {
    onDeleteMovieFile(id);

    setRemoveMovieFileModalClosed();
  }, [id, onDeleteMovieFile, setRemoveMovieFileModalClosed]);

  const showQualityPlaceholder = !quality;
  const showLanguagePlaceholder = !languages;

  return (
    <TableRow>
      {columns.map((column) => {
        const { name, isVisible } = column;

        if (!isVisible) {
          return null;
        }

        if (name === 'relativePath') {
          return (
            <TableRowCell
              key={name}
              className={styles.relativePath}
              title={relativePath}
            >
              {relativePath}
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

        if (name === 'customFormatScore') {
          return (
            <TableRowCell key={name} className={styles.customFormatScore}>
              <Tooltip
                anchor={formatCustomFormatScore(
                  customFormatScore,
                  customFormats.length
                )}
                tooltip={<MovieFormats formats={customFormats} />}
                position={tooltipPositions.LEFT}
              />
            </TableRowCell>
          );
        }

        if (name === 'indexerFlags') {
          return (
            <TableRowCell key={name} className={styles.indexerFlags}>
              {indexerFlags ? (
                <Popover
                  anchor={<Icon name={icons.FLAG} />}
                  title={translate('IndexerFlags')}
                  body={<IndexerFlags indexerFlags={indexerFlags} />}
                  position={tooltipPositions.LEFT}
                />
              ) : null}
            </TableRowCell>
          );
        }

        if (name === 'languages') {
          return (
            <TableRowCell key={name} className={styles.languages}>
              {showLanguagePlaceholder ? <MovieFileRowCellPlaceholder /> : null}

              {!showLanguagePlaceholder && !!languages && (
                <MovieLanguages languages={languages} />
              )}
            </TableRowCell>
          );
        }

        if (name === 'quality') {
          return (
            <TableRowCell key={name} className={styles.quality}>
              {showQualityPlaceholder ? <MovieFileRowCellPlaceholder /> : null}

              {!showQualityPlaceholder && !!quality && (
                <MovieQuality
                  quality={quality}
                  isCutoffNotMet={qualityCutoffNotMet}
                />
              )}
            </TableRowCell>
          );
        }

        if (name === 'audioInfo') {
          return (
            <TableRowCell key={name} className={styles.audio}>
              <MediaInfo type="audio" movieFileId={id} />
            </TableRowCell>
          );
        }

        if (name === 'audioLanguages') {
          return (
            <TableRowCell key={name} className={styles.audioLanguages}>
              <MediaInfo type="audioLanguages" movieFileId={id} />
            </TableRowCell>
          );
        }

        if (name === 'subtitleLanguages') {
          return (
            <TableRowCell key={name} className={styles.subtitles}>
              <MediaInfo type="subtitles" movieFileId={id} />
            </TableRowCell>
          );
        }

        if (name === 'videoCodec') {
          return (
            <TableRowCell key={name} className={styles.video}>
              <MediaInfo type="video" movieFileId={id} />
            </TableRowCell>
          );
        }

        if (name === 'videoDynamicRangeType') {
          return (
            <TableRowCell key={name} className={styles.videoDynamicRangeType}>
              <MediaInfo type="videoDynamicRangeType" movieFileId={id} />
            </TableRowCell>
          );
        }

        if (name === 'size') {
          return (
            <TableRowCell
              key={name}
              className={styles.size}
              title={size.toString()}
            >
              {formatBytes(size)}
            </TableRowCell>
          );
        }

        if (name === 'releaseGroup') {
          return (
            <TableRowCell key={name} className={styles.releaseGroup}>
              {releaseGroup}
            </TableRowCell>
          );
        }

        if (name === 'dateAdded') {
          return (
            <RelativeDateCell
              key={name}
              className={styles.dateAdded}
              date={dateAdded}
            />
          );
        }

        if (name === 'actions') {
          return (
            <TableRowCell key={name} className={styles.actions}>
              <IconButton
                title={translate('EditMovieFile')}
                name={icons.EDIT}
                onPress={onMovieFileEditModalPress}
              />

              {mediaInfo ? (
                <IconButton
                  title={translate('Details')}
                  name={icons.MEDIA_INFO}
                  onPress={onMovieFileDetailsModalPress}
                />
              ) : null}

              <IconButton
                title={translate('DeleteFile')}
                name={icons.REMOVE}
                onPress={setRemoveMovieFileModalOpen}
              />
            </TableRowCell>
          );
        }

        return null;
      })}

      <FileDetailsModal
        isOpen={isMovieFileDetailsModalOpen}
        mediaInfo={mediaInfo}
        onModalClose={onMovieFileDetailsModalClose}
      />

      <FileEditModal
        isOpen={isMovieFileEditModalOpen}
        movieFileId={id}
        onModalClose={onMovieFileEditModalClose}
      />

      <ConfirmModal
        isOpen={isRemoveMovieFileModalOpen}
        kind={kinds.DANGER}
        title={translate('DeleteSelectedMovieFiles')}
        message={translate('DeleteSelectedMovieFilesHelpText')}
        confirmLabel={translate('Delete')}
        onConfirm={handleRemoveMovieFilePress}
        onCancel={setRemoveMovieFileModalClosed}
      />
    </TableRow>
  );
}

export default MovieFileEditorRow;
