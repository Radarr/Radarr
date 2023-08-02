import React, { useCallback, useEffect, useMemo, useState } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { createSelector } from 'reselect';
import { useSelect } from 'App/SelectContext';
import AppState from 'App/State/AppState';
import { RENAME_MOVIE } from 'Commands/commandNames';
import SpinnerButton from 'Components/Link/SpinnerButton';
import PageContentFooter from 'Components/Page/PageContentFooter';
import usePrevious from 'Helpers/Hooks/usePrevious';
import { kinds } from 'Helpers/Props';
import { saveMovieEditor } from 'Store/Actions/movieActions';
import { fetchRootFolders } from 'Store/Actions/rootFolderActions';
import createCommandExecutingSelector from 'Store/Selectors/createCommandExecutingSelector';
import translate from 'Utilities/String/translate';
import getSelectedIds from 'Utilities/Table/getSelectedIds';
import DeleteMovieModal from './Delete/DeleteMovieModal';
import EditMoviesModal from './Edit/EditMoviesModal';
import OrganizeMoviesModal from './Organize/OrganizeMoviesModal';
import TagsModal from './Tags/TagsModal';
import styles from './MovieIndexSelectFooter.css';

interface SavePayload {
  monitored?: boolean;
  qualityProfileId?: number;
  rootFolderPath?: string;
  moveFiles?: boolean;
}

const movieEditorSelector = createSelector(
  (state: AppState) => state.movies,
  (movies) => {
    const { isSaving, isDeleting, deleteError } = movies;

    return {
      isSaving,
      isDeleting,
      deleteError,
    };
  }
);

function MovieIndexSelectFooter() {
  const { isSaving, isDeleting, deleteError } =
    useSelector(movieEditorSelector);

  const isOrganizingMovies = useSelector(
    createCommandExecutingSelector(RENAME_MOVIE)
  );

  const dispatch = useDispatch();

  const [isEditModalOpen, setIsEditModalOpen] = useState(false);
  const [isOrganizeModalOpen, setIsOrganizeModalOpen] = useState(false);
  const [isTagsModalOpen, setIsTagsModalOpen] = useState(false);
  const [isDeleteModalOpen, setIsDeleteModalOpen] = useState(false);
  const [isSavingMovies, setIsSavingMovies] = useState(false);
  const [isSavingTags, setIsSavingTags] = useState(false);
  const previousIsDeleting = usePrevious(isDeleting);

  const [selectState, selectDispatch] = useSelect();
  const { selectedState } = selectState;

  const movieIds = useMemo(() => {
    return getSelectedIds(selectedState);
  }, [selectedState]);

  const selectedCount = movieIds.length ? movieIds.length : 0;

  const onEditPress = useCallback(() => {
    setIsEditModalOpen(true);
  }, [setIsEditModalOpen]);

  const onEditModalClose = useCallback(() => {
    setIsEditModalOpen(false);
  }, [setIsEditModalOpen]);

  const onSavePress = useCallback(
    (payload: SavePayload) => {
      setIsSavingMovies(true);
      setIsEditModalOpen(false);

      dispatch(
        saveMovieEditor({
          ...payload,
          movieIds,
        })
      );
    },
    [movieIds, dispatch]
  );

  const onOrganizePress = useCallback(() => {
    setIsOrganizeModalOpen(true);
  }, [setIsOrganizeModalOpen]);

  const onOrganizeModalClose = useCallback(() => {
    setIsOrganizeModalOpen(false);
  }, [setIsOrganizeModalOpen]);

  const onTagsPress = useCallback(() => {
    setIsTagsModalOpen(true);
  }, [setIsTagsModalOpen]);

  const onTagsModalClose = useCallback(() => {
    setIsTagsModalOpen(false);
  }, [setIsTagsModalOpen]);

  const onApplyTagsPress = useCallback(
    (tags: number[], applyTags: string) => {
      setIsSavingTags(true);
      setIsTagsModalOpen(false);

      dispatch(
        saveMovieEditor({
          movieIds,
          tags,
          applyTags,
        })
      );
    },
    [movieIds, dispatch]
  );

  const onDeletePress = useCallback(() => {
    setIsDeleteModalOpen(true);
  }, [setIsDeleteModalOpen]);

  const onDeleteModalClose = useCallback(() => {
    setIsDeleteModalOpen(false);
  }, []);

  useEffect(() => {
    if (!isSaving) {
      setIsSavingMovies(false);
      setIsSavingTags(false);
    }
  }, [isSaving]);

  useEffect(() => {
    if (previousIsDeleting && !isDeleting && !deleteError) {
      selectDispatch({ type: 'unselectAll' });
    }
  }, [previousIsDeleting, isDeleting, deleteError, selectDispatch]);

  useEffect(() => {
    dispatch(fetchRootFolders());
  }, [dispatch]);

  const anySelected = selectedCount > 0;

  return (
    <PageContentFooter className={styles.footer}>
      <div className={styles.buttons}>
        <div className={styles.actionButtons}>
          <SpinnerButton
            isSpinning={isSaving && isSavingMovies}
            isDisabled={!anySelected || isOrganizingMovies}
            onPress={onEditPress}
          >
            {translate('Edit')}
          </SpinnerButton>

          <SpinnerButton
            kind={kinds.WARNING}
            isSpinning={isOrganizingMovies}
            isDisabled={!anySelected || isOrganizingMovies}
            onPress={onOrganizePress}
          >
            {translate('RenameFiles')}
          </SpinnerButton>

          <SpinnerButton
            isSpinning={isSaving && isSavingTags}
            isDisabled={!anySelected || isOrganizingMovies}
            onPress={onTagsPress}
          >
            {translate('SetTags')}
          </SpinnerButton>
        </div>

        <div className={styles.deleteButtons}>
          <SpinnerButton
            kind={kinds.DANGER}
            isSpinning={isDeleting}
            isDisabled={!anySelected || isDeleting}
            onPress={onDeletePress}
          >
            {translate('Delete')}
          </SpinnerButton>
        </div>
      </div>

      <div className={styles.selected}>
        {translate('MoviesSelectedInterp', [selectedCount])}
      </div>

      <EditMoviesModal
        isOpen={isEditModalOpen}
        movieIds={movieIds}
        onSavePress={onSavePress}
        onModalClose={onEditModalClose}
      />

      <TagsModal
        isOpen={isTagsModalOpen}
        movieIds={movieIds}
        onApplyTagsPress={onApplyTagsPress}
        onModalClose={onTagsModalClose}
      />

      <OrganizeMoviesModal
        isOpen={isOrganizeModalOpen}
        movieIds={movieIds}
        onModalClose={onOrganizeModalClose}
      />

      <DeleteMovieModal
        isOpen={isDeleteModalOpen}
        movieIds={movieIds}
        onModalClose={onDeleteModalClose}
      />
    </PageContentFooter>
  );
}

export default MovieIndexSelectFooter;
