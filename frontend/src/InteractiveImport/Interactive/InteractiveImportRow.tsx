import React, { useCallback, useEffect, useMemo, useState } from 'react';
import { useDispatch } from 'react-redux';
import Icon from 'Components/Icon';
import TableRowCell from 'Components/Table/Cells/TableRowCell';
import TableRowCellButton from 'Components/Table/Cells/TableRowCellButton';
import TableSelectCell from 'Components/Table/Cells/TableSelectCell';
import Column from 'Components/Table/Column';
import TableRow from 'Components/Table/TableRow';
import Popover from 'Components/Tooltip/Popover';
import { icons, kinds, tooltipPositions } from 'Helpers/Props';
import SelectIndexerFlagsModal from 'InteractiveImport/IndexerFlags/SelectIndexerFlagsModal';
import SelectLanguageModal from 'InteractiveImport/Language/SelectLanguageModal';
import SelectMovieModal from 'InteractiveImport/Movie/SelectMovieModal';
import SelectQualityModal from 'InteractiveImport/Quality/SelectQualityModal';
import SelectReleaseGroupModal from 'InteractiveImport/ReleaseGroup/SelectReleaseGroupModal';
import Language from 'Language/Language';
import IndexerFlags from 'Movie/IndexerFlags';
import Movie from 'Movie/Movie';
import MovieFormats from 'Movie/MovieFormats';
import MovieLanguage from 'Movie/MovieLanguage';
import MovieQuality from 'Movie/MovieQuality';
import { QualityModel } from 'Quality/Quality';
import {
  reprocessInteractiveImportItems,
  updateInteractiveImportItem,
} from 'Store/Actions/interactiveImportActions';
import { SelectStateInputProps } from 'typings/props';
import Rejection from 'typings/Rejection';
import formatBytes from 'Utilities/Number/formatBytes';
import formatCustomFormatScore from 'Utilities/Number/formatCustomFormatScore';
import translate from 'Utilities/String/translate';
import InteractiveImportRowCellPlaceholder from './InteractiveImportRowCellPlaceholder';
import styles from './InteractiveImportRow.css';

type SelectType =
  | 'movie'
  | 'releaseGroup'
  | 'quality'
  | 'language'
  | 'indexerFlags';

type SelectedChangeProps = SelectStateInputProps & {
  hasMovieFileId: boolean;
};

interface InteractiveImportRowProps {
  id: number;
  allowMovieChange: boolean;
  relativePath: string;
  movie?: Movie;
  releaseGroup?: string;
  quality?: QualityModel;
  languages?: Language[];
  size: number;
  customFormats?: object[];
  customFormatScore?: number;
  indexerFlags: number;
  rejections: Rejection[];
  columns: Column[];
  movieFileId?: number;
  isReprocessing?: boolean;
  isSelected?: boolean;
  modalTitle: string;
  onSelectedChange(result: SelectedChangeProps): void;
  onValidRowChange(id: number, isValid: boolean): void;
}

function InteractiveImportRow(props: InteractiveImportRowProps) {
  const {
    id,
    allowMovieChange,
    relativePath,
    movie,
    quality,
    languages,
    releaseGroup,
    size,
    customFormats,
    customFormatScore,
    indexerFlags,
    rejections,
    isSelected,
    modalTitle,
    movieFileId,
    columns,
    onSelectedChange,
    onValidRowChange,
  } = props;

  const dispatch = useDispatch();

  const isMovieColumnVisible = useMemo(
    () => columns.find((c) => c.name === 'movie')?.isVisible ?? false,
    [columns]
  );
  const isIndexerFlagsColumnVisible = useMemo(
    () => columns.find((c) => c.name === 'indexerFlags')?.isVisible ?? false,
    [columns]
  );

  const [selectModalOpen, setSelectModalOpen] = useState<SelectType | null>(
    null
  );

  useEffect(
    () => {
      if (allowMovieChange && movie && quality && languages && size > 0) {
        onSelectedChange({
          id,
          hasMovieFileId: !!movieFileId,
          value: true,
          shiftKey: false,
        });
      }
    },
    // eslint-disable-next-line react-hooks/exhaustive-deps
    []
  );

  useEffect(() => {
    const isValid = !!(movie && quality && languages);

    if (isSelected && !isValid) {
      onValidRowChange(id, false);
    } else {
      onValidRowChange(id, true);
    }
  }, [id, movie, quality, languages, isSelected, onValidRowChange]);

  const onSelectedChangeWrapper = useCallback(
    (result: SelectedChangeProps) => {
      onSelectedChange({
        ...result,
        hasMovieFileId: !!movieFileId,
      });
    },
    [movieFileId, onSelectedChange]
  );

  const selectRowAfterChange = useCallback(() => {
    if (!isSelected) {
      onSelectedChange({
        id,
        hasMovieFileId: !!movieFileId,
        value: true,
        shiftKey: false,
      });
    }
  }, [id, movieFileId, isSelected, onSelectedChange]);

  const onSelectModalClose = useCallback(() => {
    setSelectModalOpen(null);
  }, [setSelectModalOpen]);

  const onSelectMoviePress = useCallback(() => {
    setSelectModalOpen('movie');
  }, [setSelectModalOpen]);

  const onMovieSelect = useCallback(
    (movie: Movie) => {
      dispatch(
        updateInteractiveImportItem({
          id,
          movie,
        })
      );

      dispatch(reprocessInteractiveImportItems({ ids: [id] }));

      setSelectModalOpen(null);
      selectRowAfterChange();
    },
    [id, dispatch, setSelectModalOpen, selectRowAfterChange]
  );

  const onSelectReleaseGroupPress = useCallback(() => {
    setSelectModalOpen('releaseGroup');
  }, [setSelectModalOpen]);

  const onReleaseGroupSelect = useCallback(
    (releaseGroup: string) => {
      dispatch(
        updateInteractiveImportItem({
          id,
          releaseGroup,
        })
      );

      dispatch(reprocessInteractiveImportItems({ ids: [id] }));

      setSelectModalOpen(null);
      selectRowAfterChange();
    },
    [id, dispatch, setSelectModalOpen, selectRowAfterChange]
  );

  const onSelectQualityPress = useCallback(() => {
    setSelectModalOpen('quality');
  }, [setSelectModalOpen]);

  const onQualitySelect = useCallback(
    (quality: QualityModel) => {
      dispatch(
        updateInteractiveImportItem({
          id,
          quality,
        })
      );

      dispatch(reprocessInteractiveImportItems({ ids: [id] }));

      setSelectModalOpen(null);
      selectRowAfterChange();
    },
    [id, dispatch, setSelectModalOpen, selectRowAfterChange]
  );

  const onSelectLanguagePress = useCallback(() => {
    setSelectModalOpen('language');
  }, [setSelectModalOpen]);

  const onLanguagesSelect = useCallback(
    (languages: Language[]) => {
      dispatch(
        updateInteractiveImportItem({
          id,
          languages,
        })
      );

      dispatch(reprocessInteractiveImportItems({ ids: [id] }));

      setSelectModalOpen(null);
      selectRowAfterChange();
    },
    [id, dispatch, setSelectModalOpen, selectRowAfterChange]
  );

  const onSelectIndexerFlagsPress = useCallback(() => {
    setSelectModalOpen('indexerFlags');
  }, [setSelectModalOpen]);

  const onIndexerFlagsSelect = useCallback(
    (indexerFlags: number) => {
      dispatch(
        updateInteractiveImportItem({
          id,
          indexerFlags,
        })
      );

      dispatch(reprocessInteractiveImportItems({ ids: [id] }));

      setSelectModalOpen(null);
      selectRowAfterChange();
    },
    [id, dispatch, setSelectModalOpen, selectRowAfterChange]
  );

  const movieTitle = movie ? movie.title : '';

  const showMoviePlaceholder = isSelected && !movie;
  const showReleaseGroupPlaceholder = isSelected && !releaseGroup;
  const showQualityPlaceholder = isSelected && !quality;
  const showLanguagePlaceholder = isSelected && !languages;
  const showIndexerFlagsPlaceholder = isSelected && !indexerFlags;

  return (
    <TableRow>
      <TableSelectCell
        id={id}
        isSelected={isSelected}
        onSelectedChange={onSelectedChangeWrapper}
      />

      <TableRowCell className={styles.relativePath} title={relativePath}>
        {relativePath}
      </TableRowCell>

      {isMovieColumnVisible ? (
        <TableRowCellButton
          isDisabled={!allowMovieChange}
          title={allowMovieChange ? translate('ClickToChangeMovie') : undefined}
          onPress={onSelectMoviePress}
        >
          {showMoviePlaceholder ? (
            <InteractiveImportRowCellPlaceholder />
          ) : (
            movieTitle
          )}
        </TableRowCellButton>
      ) : null}

      <TableRowCellButton
        title={translate('ClickToChangeReleaseGroup')}
        onPress={onSelectReleaseGroupPress}
      >
        {showReleaseGroupPlaceholder ? (
          <InteractiveImportRowCellPlaceholder isOptional={true} />
        ) : (
          releaseGroup
        )}
      </TableRowCellButton>

      <TableRowCellButton
        className={styles.quality}
        title={translate('ClickToChangeQuality')}
        onPress={onSelectQualityPress}
      >
        {showQualityPlaceholder && <InteractiveImportRowCellPlaceholder />}

        {!showQualityPlaceholder && !!quality && (
          <MovieQuality className={styles.label} quality={quality} />
        )}
      </TableRowCellButton>

      <TableRowCellButton
        className={styles.languages}
        title={translate('ClickToChangeLanguage')}
        onPress={onSelectLanguagePress}
      >
        {showLanguagePlaceholder && <InteractiveImportRowCellPlaceholder />}

        {!showLanguagePlaceholder && !!languages && (
          <MovieLanguage className={styles.label} languages={languages} />
        )}
      </TableRowCellButton>

      <TableRowCell>{formatBytes(size)}</TableRowCell>

      <TableRowCell>
        {customFormats?.length ? (
          <Popover
            anchor={formatCustomFormatScore(
              customFormatScore,
              customFormats.length
            )}
            title={translate('CustomFormats')}
            body={
              <div className={styles.customFormatTooltip}>
                <MovieFormats formats={customFormats} />
              </div>
            }
            position={tooltipPositions.LEFT}
          />
        ) : null}
      </TableRowCell>

      {isIndexerFlagsColumnVisible ? (
        <TableRowCellButton
          title={translate('ClickToChangeIndexerFlags')}
          onPress={onSelectIndexerFlagsPress}
        >
          {showIndexerFlagsPlaceholder ? (
            <InteractiveImportRowCellPlaceholder isOptional={true} />
          ) : (
            <>
              {indexerFlags ? (
                <Popover
                  anchor={<Icon name={icons.FLAG} kind={kinds.PRIMARY} />}
                  title={translate('IndexerFlags')}
                  body={<IndexerFlags indexerFlags={indexerFlags} />}
                  position={tooltipPositions.LEFT}
                />
              ) : null}
            </>
          )}
        </TableRowCellButton>
      ) : null}

      <TableRowCell>
        {rejections.length ? (
          <Popover
            anchor={<Icon name={icons.DANGER} kind={kinds.DANGER} />}
            title={translate('ReleaseRejected')}
            body={
              <ul>
                {rejections.map((rejection, index) => {
                  return <li key={index}>{rejection.reason}</li>;
                })}
              </ul>
            }
            position={tooltipPositions.LEFT}
            canFlip={false}
          />
        ) : null}
      </TableRowCell>

      <SelectMovieModal
        isOpen={selectModalOpen === 'movie'}
        modalTitle={modalTitle}
        onMovieSelect={onMovieSelect}
        onModalClose={onSelectModalClose}
      />

      <SelectReleaseGroupModal
        isOpen={selectModalOpen === 'releaseGroup'}
        releaseGroup={releaseGroup ?? ''}
        modalTitle={modalTitle}
        onReleaseGroupSelect={onReleaseGroupSelect}
        onModalClose={onSelectModalClose}
      />

      <SelectQualityModal
        isOpen={selectModalOpen === 'quality'}
        qualityId={quality ? quality.quality.id : 0}
        proper={quality ? quality.revision.version > 1 : false}
        real={quality ? quality.revision.real > 0 : false}
        modalTitle={modalTitle}
        onQualitySelect={onQualitySelect}
        onModalClose={onSelectModalClose}
      />

      <SelectLanguageModal
        isOpen={selectModalOpen === 'language'}
        languageIds={languages ? languages.map((l) => l.id) : []}
        modalTitle={modalTitle}
        onLanguagesSelect={onLanguagesSelect}
        onModalClose={onSelectModalClose}
      />

      <SelectIndexerFlagsModal
        isOpen={selectModalOpen === 'indexerFlags'}
        indexerFlags={indexerFlags ?? 0}
        modalTitle={modalTitle}
        onIndexerFlagsSelect={onIndexerFlagsSelect}
        onModalClose={onSelectModalClose}
      />
    </TableRow>
  );
}

export default InteractiveImportRow;
