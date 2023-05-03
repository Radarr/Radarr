import { orderBy } from 'lodash';
import React, { useCallback, useMemo, useState } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { createSelector } from 'reselect';
import AppState from 'App/State/AppState';
import FormGroup from 'Components/Form/FormGroup';
import FormInputGroup from 'Components/Form/FormInputGroup';
import FormLabel from 'Components/Form/FormLabel';
import Button from 'Components/Link/Button';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import { inputTypes, kinds } from 'Helpers/Props';
import Movie from 'Movie/Movie';
import { bulkDeleteMovie, setDeleteOption } from 'Store/Actions/movieActions';
import createAllMoviesSelector from 'Store/Selectors/createAllMoviesSelector';
import { CheckInputChanged } from 'typings/inputs';
import translate from 'Utilities/String/translate';
import styles from './DeleteMovieModalContent.css';

interface DeleteMovieModalContentProps {
  movieIds: number[];
  onModalClose(): void;
}

const selectDeleteOptions = createSelector(
  (state: AppState) => state.movies.deleteOptions,
  (deleteOptions) => deleteOptions
);

function DeleteMovieModalContent(props: DeleteMovieModalContentProps) {
  const { movieIds, onModalClose } = props;

  const { addImportExclusion } = useSelector(selectDeleteOptions);
  const allMovies: Movie[] = useSelector(createAllMoviesSelector());
  const dispatch = useDispatch();

  const [deleteFiles, setDeleteFiles] = useState(false);

  const movies = useMemo((): Movie[] => {
    const movies = movieIds.map((id) => {
      return allMovies.find((s) => s.id === id);
    }) as Movie[];

    return orderBy(movies, ['sortTitle']);
  }, [movieIds, allMovies]);

  const onDeleteFilesChange = useCallback(
    ({ value }: CheckInputChanged) => {
      setDeleteFiles(value);
    },
    [setDeleteFiles]
  );

  const onDeleteOptionChange = useCallback(
    ({ name, value }: { name: string; value: boolean }) => {
      dispatch(
        setDeleteOption({
          [name]: value,
        })
      );
    },
    [dispatch]
  );

  const onDeleteMoviesConfirmed = useCallback(() => {
    setDeleteFiles(false);

    dispatch(
      bulkDeleteMovie({
        movieIds,
        deleteFiles,
        addImportExclusion,
      })
    );

    onModalClose();
  }, [
    movieIds,
    deleteFiles,
    addImportExclusion,
    setDeleteFiles,
    dispatch,
    onModalClose,
  ]);

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>{translate('DeleteSelectedMovie')}</ModalHeader>

      <ModalBody>
        <div>
          <FormGroup>
            <FormLabel>{translate('AddListExclusion')}</FormLabel>

            <FormInputGroup
              type={inputTypes.CHECK}
              name="addImportExclusion"
              value={addImportExclusion}
              helpText={translate('AddImportExclusionHelpText')}
              onChange={onDeleteOptionChange}
            />
          </FormGroup>

          <FormGroup>
            <FormLabel>{`Delete Movie Folder${
              movies.length > 1 ? 's' : ''
            }`}</FormLabel>

            <FormInputGroup
              type={inputTypes.CHECK}
              name="deleteFiles"
              value={deleteFiles}
              helpText={`Delete Movie Folder${
                movies.length > 1 ? 's' : ''
              } and all contents`}
              kind={kinds.DANGER}
              onChange={onDeleteFilesChange}
            />
          </FormGroup>
        </div>

        <div className={styles.message}>
          {`Are you sure you want to delete ${movies.length} selected movie(s)${
            deleteFiles ? ' and all contents' : ''
          }?`}
        </div>

        <ul>
          {movies.map((s) => {
            return (
              <li key={s.title}>
                <span>{s.title}</span>

                {deleteFiles && (
                  <span className={styles.pathContainer}>
                    -<span className={styles.path}>{s.path}</span>
                  </span>
                )}
              </li>
            );
          })}
        </ul>
      </ModalBody>

      <ModalFooter>
        <Button onPress={onModalClose}>{translate('Cancel')}</Button>

        <Button kind={kinds.DANGER} onPress={onDeleteMoviesConfirmed}>
          {translate('Delete')}
        </Button>
      </ModalFooter>
    </ModalContent>
  );
}

export default DeleteMovieModalContent;
