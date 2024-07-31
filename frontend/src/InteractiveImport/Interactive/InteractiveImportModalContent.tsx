import { cloneDeep, without } from 'lodash';
import React, { useCallback, useEffect, useMemo, useState } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { createSelector } from 'reselect';
import AppState from 'App/State/AppState';
import InteractiveImportAppState from 'App/State/InteractiveImportAppState';
import * as commandNames from 'Commands/commandNames';
import SelectInput from 'Components/Form/SelectInput';
import Icon from 'Components/Icon';
import Button from 'Components/Link/Button';
import SpinnerButton from 'Components/Link/SpinnerButton';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import Menu from 'Components/Menu/Menu';
import MenuButton from 'Components/Menu/MenuButton';
import MenuContent from 'Components/Menu/MenuContent';
import SelectedMenuItem from 'Components/Menu/SelectedMenuItem';
import ConfirmModal from 'Components/Modal/ConfirmModal';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import Column from 'Components/Table/Column';
import Table from 'Components/Table/Table';
import TableBody from 'Components/Table/TableBody';
import usePrevious from 'Helpers/Hooks/usePrevious';
import useSelectState from 'Helpers/Hooks/useSelectState';
import { align, icons, kinds, scrollDirections } from 'Helpers/Props';
import ImportMode from 'InteractiveImport/ImportMode';
import SelectIndexerFlagsModal from 'InteractiveImport/IndexerFlags/SelectIndexerFlagsModal';
import InteractiveImport, {
  InteractiveImportCommandOptions,
} from 'InteractiveImport/InteractiveImport';
import SelectLanguageModal from 'InteractiveImport/Language/SelectLanguageModal';
import SelectMovieModal from 'InteractiveImport/Movie/SelectMovieModal';
import SelectQualityModal from 'InteractiveImport/Quality/SelectQualityModal';
import SelectReleaseGroupModal from 'InteractiveImport/ReleaseGroup/SelectReleaseGroupModal';
import Language from 'Language/Language';
import Movie from 'Movie/Movie';
import { MovieFile } from 'MovieFile/MovieFile';
import { QualityModel } from 'Quality/Quality';
import { executeCommand } from 'Store/Actions/commandActions';
import {
  clearInteractiveImport,
  fetchInteractiveImportItems,
  reprocessInteractiveImportItems,
  setInteractiveImportMode,
  setInteractiveImportSort,
  updateInteractiveImportItems,
} from 'Store/Actions/interactiveImportActions';
import {
  deleteMovieFiles,
  updateMovieFiles,
} from 'Store/Actions/movieFileActions';
import createClientSideCollectionSelector from 'Store/Selectors/createClientSideCollectionSelector';
import { SortCallback } from 'typings/callbacks';
import { SelectStateInputProps } from 'typings/props';
import getErrorMessage from 'Utilities/Object/getErrorMessage';
import translate from 'Utilities/String/translate';
import getSelectedIds from 'Utilities/Table/getSelectedIds';
import InteractiveImportRow from './InteractiveImportRow';
import styles from './InteractiveImportModalContent.css';

type SelectType =
  | 'select'
  | 'movie'
  | 'releaseGroup'
  | 'quality'
  | 'language'
  | 'indexerFlags';

type FilterExistingFiles = 'all' | 'new';

// TODO: This feels janky to do, but not sure of a better way currently
type OnSelectedChangeCallback = React.ComponentProps<
  typeof InteractiveImportRow
>['onSelectedChange'];

const COLUMNS = [
  {
    name: 'relativePath',
    label: () => translate('RelativePath'),
    isSortable: true,
    isVisible: true,
  },
  {
    name: 'movie',
    label: () => translate('Movie'),
    isSortable: true,
    isVisible: true,
  },
  {
    name: 'releaseGroup',
    label: () => translate('ReleaseGroup'),
    isVisible: true,
  },
  {
    name: 'quality',
    label: () => translate('Quality'),
    isSortable: true,
    isVisible: true,
  },
  {
    name: 'languages',
    label: () => translate('Languages'),
    isSortable: true,
    isVisible: true,
  },
  {
    name: 'size',
    label: () => translate('Size'),
    isSortable: true,
    isVisible: true,
  },
  {
    name: 'customFormats',
    label: React.createElement(Icon, {
      name: icons.INTERACTIVE,
      title: () => translate('CustomFormat'),
    }),
    isSortable: true,
    isVisible: true,
  },
  {
    name: 'indexerFlags',
    label: React.createElement(Icon, {
      name: icons.FLAG,
      title: () => translate('IndexerFlags'),
    }),
    isSortable: true,
    isVisible: true,
  },
  {
    name: 'rejections',
    label: React.createElement(Icon, {
      name: icons.DANGER,
      kind: kinds.DANGER,
      title: () => translate('Rejections'),
    }),
    isSortable: true,
    isVisible: true,
  },
];

const importModeOptions = [
  {
    key: 'chooseImportMode',
    value: () => translate('ChooseImportMode'),
    disabled: true,
  },
  {
    key: 'move',
    value: () => translate('MoveFiles'),
  },
  {
    key: 'copy',
    value: () => translate('HardlinkCopyFiles'),
  },
];

function isSameMovieFile(
  file: InteractiveImport,
  originalFile?: InteractiveImport
) {
  const { movie } = file;

  if (!originalFile) {
    return false;
  }

  if (!originalFile.movie || movie?.id !== originalFile.movie.id) {
    return false;
  }

  return true;
}

const movieFilesInfoSelector = createSelector(
  (state: AppState) => state.movieFiles.isDeleting,
  (state: AppState) => state.movieFiles.deleteError,
  (isDeleting, deleteError) => {
    return {
      isDeleting,
      deleteError,
    };
  }
);

const importModeSelector = createSelector(
  (state: AppState) => state.interactiveImport.importMode,
  (importMode) => {
    return importMode;
  }
);

interface InteractiveImportModalContentProps {
  downloadId?: string;
  movieId?: number;
  seasonNumber?: number;
  showMovie?: boolean;
  allowMovieChange?: boolean;
  showDelete?: boolean;
  showImportMode?: boolean;
  showFilterExistingFiles?: boolean;
  title?: string;
  folder?: string;
  sortKey?: string;
  sortDirection?: string;
  initialSortKey?: string;
  initialSortDirection?: string;
  modalTitle: string;
  onModalClose(): void;
}

function InteractiveImportModalContent(
  props: InteractiveImportModalContentProps
) {
  const {
    downloadId,
    movieId,
    seasonNumber,
    allowMovieChange = true,
    showMovie = true,
    showFilterExistingFiles = false,
    showDelete = false,
    showImportMode = true,
    title,
    folder,
    initialSortKey,
    initialSortDirection,
    modalTitle,
    onModalClose,
  } = props;

  const {
    isFetching,
    isPopulated,
    error,
    items,
    originalItems,
    sortKey,
    sortDirection,
  }: InteractiveImportAppState = useSelector(
    createClientSideCollectionSelector('interactiveImport')
  );

  const { isDeleting, deleteError } = useSelector(movieFilesInfoSelector);
  const importMode = useSelector(importModeSelector);

  const [invalidRowsSelected, setInvalidRowsSelected] = useState<number[]>([]);
  const [withoutMovieFileIdRowsSelected, setWithoutMovieFileIdRowsSelected] =
    useState<number[]>([]);
  const [selectModalOpen, setSelectModalOpen] = useState<SelectType | null>(
    null
  );
  const [isConfirmDeleteModalOpen, setIsConfirmDeleteModalOpen] =
    useState(false);
  const [filterExistingFiles, setFilterExistingFiles] = useState(false);
  const [interactiveImportErrorMessage, setInteractiveImportErrorMessage] =
    useState<string | null>(null);
  const [selectState, setSelectState] = useSelectState();
  const { allSelected, allUnselected, selectedState } = selectState;
  const previousIsDeleting = usePrevious(isDeleting);
  const dispatch = useDispatch();

  const columns: Column[] = useMemo(() => {
    const result: Column[] = cloneDeep(COLUMNS);

    if (!showMovie) {
      const movieColumn = result.find((c) => c.name === 'movie');

      if (movieColumn) {
        movieColumn.isVisible = false;
      }
    }

    const showIndexerFlags = items.some((item) => item.indexerFlags);

    if (!showIndexerFlags) {
      const indexerFlagsColumn = result.find((c) => c.name === 'indexerFlags');

      if (indexerFlagsColumn) {
        indexerFlagsColumn.isVisible = false;
      }
    }

    return result;
  }, [showMovie, items]);

  const selectedIds: number[] = useMemo(() => {
    return getSelectedIds(selectedState);
  }, [selectedState]);

  const bulkSelectOptions = useMemo(() => {
    const options = [
      {
        key: 'select',
        value: translate('SelectDropdown'),
        disabled: true,
      },
      {
        key: 'quality',
        value: translate('SelectQuality'),
      },
      {
        key: 'releaseGroup',
        value: translate('SelectReleaseGroup'),
      },
      {
        key: 'language',
        value: translate('SelectLanguage'),
      },
      {
        key: 'indexerFlags',
        value: translate('SelectIndexerFlags'),
      },
    ];

    if (allowMovieChange) {
      options.splice(1, 0, {
        key: 'movie',
        value: translate('SelectMovie'),
      });
    }

    return options;
  }, [allowMovieChange]);

  useEffect(
    () => {
      if (initialSortKey) {
        const sortProps: { sortKey: string; sortDirection?: string } = {
          sortKey: initialSortKey,
        };

        if (initialSortDirection) {
          sortProps.sortDirection = initialSortDirection;
        }

        dispatch(setInteractiveImportSort(sortProps));
      }

      dispatch(
        fetchInteractiveImportItems({
          downloadId,
          movieId,
          seasonNumber,
          folder,
          filterExistingFiles,
        })
      );

      // returned function will be called on component unmount
      return () => {
        dispatch(clearInteractiveImport());
      };
    },
    // eslint-disable-next-line react-hooks/exhaustive-deps
    []
  );

  useEffect(() => {
    if (!isDeleting && previousIsDeleting && !deleteError) {
      onModalClose();
    }
  }, [previousIsDeleting, isDeleting, deleteError, onModalClose]);

  const onSelectAllChange = useCallback(
    ({ value }: SelectStateInputProps) => {
      setSelectState({ type: value ? 'selectAll' : 'unselectAll', items });
    },
    [items, setSelectState]
  );

  const onSelectedChange = useCallback<OnSelectedChangeCallback>(
    ({ id, value, hasMovieFileId, shiftKey = false }) => {
      setSelectState({
        type: 'toggleSelected',
        items,
        id,
        isSelected: value,
        shiftKey,
      });

      setWithoutMovieFileIdRowsSelected(
        hasMovieFileId || !value
          ? without(withoutMovieFileIdRowsSelected, id)
          : [...withoutMovieFileIdRowsSelected, id]
      );
    },
    [
      items,
      withoutMovieFileIdRowsSelected,
      setSelectState,
      setWithoutMovieFileIdRowsSelected,
    ]
  );

  const onValidRowChange = useCallback(
    (id: number, isValid: boolean) => {
      if (isValid && invalidRowsSelected.includes(id)) {
        setInvalidRowsSelected(without(invalidRowsSelected, id));
      } else if (!isValid && !invalidRowsSelected.includes(id)) {
        setInvalidRowsSelected([...invalidRowsSelected, id]);
      }
    },
    [invalidRowsSelected, setInvalidRowsSelected]
  );

  const onDeleteSelectedPress = useCallback(() => {
    setIsConfirmDeleteModalOpen(true);
  }, [setIsConfirmDeleteModalOpen]);

  const onConfirmDelete = useCallback(() => {
    setIsConfirmDeleteModalOpen(false);

    const movieFileIds = items.reduce((acc: number[], item) => {
      if (selectedIds.indexOf(item.id) > -1 && item.movieFileId) {
        acc.push(item.movieFileId);
      }

      return acc;
    }, []);

    dispatch(deleteMovieFiles({ movieFileIds }));
  }, [items, selectedIds, setIsConfirmDeleteModalOpen, dispatch]);

  const onConfirmDeleteModalClose = useCallback(() => {
    setIsConfirmDeleteModalOpen(false);
  }, [setIsConfirmDeleteModalOpen]);

  const onImportSelectedPress = useCallback(() => {
    const finalImportMode = downloadId || !showImportMode ? 'auto' : importMode;

    const existingFiles: Partial<MovieFile>[] = [];
    const files: InteractiveImportCommandOptions[] = [];

    if (finalImportMode === 'chooseImportMode') {
      setInteractiveImportErrorMessage(
        translate('InteractiveImportNoImportMode')
      );

      return;
    }

    items.forEach((item) => {
      const isSelected = selectedIds.indexOf(item.id) > -1;

      if (isSelected) {
        const {
          movie,
          releaseGroup,
          quality,
          languages,
          indexerFlags,
          movieFileId,
        } = item;

        if (!movie) {
          setInteractiveImportErrorMessage(
            translate('InteractiveImportNoMovie')
          );
          return;
        }

        if (!quality) {
          setInteractiveImportErrorMessage(
            translate('InteractiveImportNoQuality')
          );
          return;
        }

        if (!languages) {
          setInteractiveImportErrorMessage(
            translate('InteractiveImportNoLanguage')
          );
          return;
        }

        setInteractiveImportErrorMessage(null);

        if (movieFileId) {
          const originalItem = originalItems.find((i) => i.id === item.id);

          if (isSameMovieFile(item, originalItem)) {
            existingFiles.push({
              id: movieFileId,
              releaseGroup,
              quality,
              languages,
              indexerFlags,
            });

            return;
          }
        }

        files.push({
          path: item.path,
          folderName: item.folderName,
          movieId: movie.id,
          releaseGroup,
          quality,
          languages,
          indexerFlags,
          downloadId,
          movieFileId,
        });
      }
    });

    let shouldClose = false;

    if (existingFiles.length) {
      dispatch(
        updateMovieFiles({
          files: existingFiles,
        })
      );

      shouldClose = true;
    }

    if (files.length) {
      dispatch(
        executeCommand({
          name: commandNames.INTERACTIVE_IMPORT,
          files,
          importMode: finalImportMode,
        })
      );

      shouldClose = true;
    }

    if (shouldClose) {
      onModalClose();
    }
  }, [
    downloadId,
    showImportMode,
    importMode,
    items,
    originalItems,
    selectedIds,
    onModalClose,
    dispatch,
  ]);

  const onSortPress = useCallback<SortCallback>(
    (sortKey, sortDirection) => {
      dispatch(setInteractiveImportSort({ sortKey, sortDirection }));
    },
    [dispatch]
  );

  const onFilterExistingFilesChange = useCallback<
    (value: FilterExistingFiles) => void
  >(
    (value) => {
      const filter = value !== 'all';

      setFilterExistingFiles(filter);

      dispatch(
        fetchInteractiveImportItems({
          downloadId,
          movieId,
          folder,
          filterExistingFiles: filter,
        })
      );
    },
    [downloadId, movieId, folder, setFilterExistingFiles, dispatch]
  );

  const onImportModeChange = useCallback<
    ({ value }: { value: ImportMode }) => void
  >(
    ({ value }) => {
      dispatch(setInteractiveImportMode({ importMode: value }));
    },
    [dispatch]
  );

  const onSelectModalSelect = useCallback<
    ({ value }: { value: SelectType }) => void
  >(
    ({ value }) => {
      setSelectModalOpen(value);
    },
    [setSelectModalOpen]
  );

  const onSelectModalClose = useCallback(() => {
    setSelectModalOpen(null);
  }, [setSelectModalOpen]);

  const onMovieSelect = useCallback(
    (movie: Movie) => {
      dispatch(
        updateInteractiveImportItems({
          ids: selectedIds,
          movie,
        })
      );

      dispatch(reprocessInteractiveImportItems({ ids: selectedIds }));

      setSelectModalOpen(null);
    },
    [selectedIds, setSelectModalOpen, dispatch]
  );

  const onReleaseGroupSelect = useCallback(
    (releaseGroup: string) => {
      dispatch(
        updateInteractiveImportItems({
          ids: selectedIds,
          releaseGroup,
        })
      );

      dispatch(reprocessInteractiveImportItems({ ids: selectedIds }));

      setSelectModalOpen(null);
    },
    [selectedIds, dispatch]
  );

  const onLanguagesSelect = useCallback(
    (newLanguages: Language[]) => {
      dispatch(
        updateInteractiveImportItems({
          ids: selectedIds,
          languages: newLanguages,
        })
      );

      dispatch(reprocessInteractiveImportItems({ ids: selectedIds }));

      setSelectModalOpen(null);
    },
    [selectedIds, dispatch]
  );

  const onQualitySelect = useCallback(
    (quality: QualityModel) => {
      dispatch(
        updateInteractiveImportItems({
          ids: selectedIds,
          quality,
        })
      );

      dispatch(reprocessInteractiveImportItems({ ids: selectedIds }));

      setSelectModalOpen(null);
    },
    [selectedIds, dispatch]
  );

  const onIndexerFlagsSelect = useCallback(
    (indexerFlags: number) => {
      dispatch(
        updateInteractiveImportItems({
          ids: selectedIds,
          indexerFlags,
        })
      );

      dispatch(reprocessInteractiveImportItems({ ids: selectedIds }));

      setSelectModalOpen(null);
    },
    [selectedIds, dispatch]
  );

  const errorMessage = getErrorMessage(
    error,
    translate('InteractiveImportLoadError')
  );

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>
        {modalTitle} - {title || folder}
      </ModalHeader>

      <ModalBody scrollDirection={scrollDirections.BOTH}>
        {showFilterExistingFiles && (
          <div className={styles.filterContainer}>
            <Menu alignMenu={align.RIGHT}>
              <MenuButton>
                <Icon name={icons.FILTER} size={22} />

                <div className={styles.filterText}>
                  {filterExistingFiles
                    ? translate('UnmappedFilesOnly')
                    : translate('AllFiles')}
                </div>
              </MenuButton>

              <MenuContent>
                <SelectedMenuItem
                  name="all"
                  isSelected={!filterExistingFiles}
                  onPress={onFilterExistingFilesChange}
                >
                  {translate('AllFiles')}
                </SelectedMenuItem>

                <SelectedMenuItem
                  name="new"
                  isSelected={filterExistingFiles}
                  onPress={onFilterExistingFilesChange}
                >
                  {translate('UnmappedFilesOnly')}
                </SelectedMenuItem>
              </MenuContent>
            </Menu>
          </div>
        )}

        {isFetching ? <LoadingIndicator /> : null}

        {error ? <div>{errorMessage}</div> : null}

        {isPopulated && !!items.length && !isFetching && !isFetching ? (
          <Table
            columns={columns}
            horizontalScroll={true}
            selectAll={true}
            allSelected={allSelected}
            allUnselected={allUnselected}
            sortKey={sortKey}
            sortDirection={sortDirection}
            onSortPress={onSortPress}
            onSelectAllChange={onSelectAllChange}
          >
            <TableBody>
              {items.map((item) => {
                return (
                  <InteractiveImportRow
                    key={item.id}
                    isSelected={selectedState[item.id]}
                    {...item}
                    allowMovieChange={allowMovieChange}
                    columns={columns}
                    modalTitle={modalTitle}
                    onSelectedChange={onSelectedChange}
                    onValidRowChange={onValidRowChange}
                  />
                );
              })}
            </TableBody>
          </Table>
        ) : null}

        {isPopulated && !items.length && !isFetching
          ? translate('InteractiveImportNoFilesFound')
          : null}
      </ModalBody>

      <ModalFooter className={styles.footer}>
        <div className={styles.leftButtons}>
          {showDelete ? (
            <SpinnerButton
              className={styles.deleteButton}
              kind={kinds.DANGER}
              isSpinning={isDeleting}
              isDisabled={
                !selectedIds.length || !!withoutMovieFileIdRowsSelected.length
              }
              onPress={onDeleteSelectedPress}
            >
              {translate('Delete')}
            </SpinnerButton>
          ) : null}

          {!downloadId && showImportMode ? (
            <SelectInput
              className={styles.importMode}
              name="importMode"
              value={importMode}
              values={importModeOptions}
              onChange={onImportModeChange}
            />
          ) : null}

          <SelectInput
            className={styles.bulkSelect}
            name="select"
            value="select"
            values={bulkSelectOptions}
            isDisabled={!selectedIds.length}
            onChange={onSelectModalSelect}
          />
        </div>

        <div className={styles.rightButtons}>
          <Button onPress={onModalClose}>{translate('Cancel')}</Button>

          {interactiveImportErrorMessage && (
            <span className={styles.errorMessage}>
              {interactiveImportErrorMessage}
            </span>
          )}

          <Button
            kind={kinds.SUCCESS}
            isDisabled={!selectedIds.length || !!invalidRowsSelected.length}
            onPress={onImportSelectedPress}
          >
            {translate('Import')}
          </Button>
        </div>
      </ModalFooter>

      <SelectMovieModal
        isOpen={selectModalOpen === 'movie'}
        modalTitle={modalTitle}
        onMovieSelect={onMovieSelect}
        onModalClose={onSelectModalClose}
      />

      <SelectReleaseGroupModal
        isOpen={selectModalOpen === 'releaseGroup'}
        releaseGroup=""
        modalTitle={modalTitle}
        onReleaseGroupSelect={onReleaseGroupSelect}
        onModalClose={onSelectModalClose}
      />

      <SelectLanguageModal
        isOpen={selectModalOpen === 'language'}
        languageIds={[0]}
        modalTitle={modalTitle}
        onLanguagesSelect={onLanguagesSelect}
        onModalClose={onSelectModalClose}
      />

      <SelectQualityModal
        isOpen={selectModalOpen === 'quality'}
        qualityId={0}
        proper={false}
        real={false}
        modalTitle={modalTitle}
        onQualitySelect={onQualitySelect}
        onModalClose={onSelectModalClose}
      />

      <SelectIndexerFlagsModal
        isOpen={selectModalOpen === 'indexerFlags'}
        indexerFlags={0}
        modalTitle={modalTitle}
        onIndexerFlagsSelect={onIndexerFlagsSelect}
        onModalClose={onSelectModalClose}
      />

      <ConfirmModal
        isOpen={isConfirmDeleteModalOpen}
        kind={kinds.DANGER}
        title={translate('DeleteSelectedMovieFiles')}
        message={translate('DeleteSelectedMovieFilesHelpText')}
        confirmLabel={translate('Delete')}
        onConfirm={onConfirmDelete}
        onCancel={onConfirmDeleteModalClose}
      />
    </ModalContent>
  );
}

export default InteractiveImportModalContent;
