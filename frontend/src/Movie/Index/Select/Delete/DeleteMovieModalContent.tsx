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
import formatBytes from 'Utilities/Number/formatBytes';
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

  const { totalMovieFileCount, totalSizeOnDisk } = useMemo(() => {
    return movies.reduce(
      (acc, { statistics = {} }) => {
        const { movieFileCount = 0, sizeOnDisk = 0 } = statistics;

        acc.totalMovieFileCount += movieFileCount;
        acc.totalSizeOnDisk += sizeOnDisk;

        return acc;
      },
      {
        totalMovieFileCount: 0,
        totalSizeOnDisk: 0,
      }
    );
  }, [movies]);

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>
        {movies.length > 1
          ? translate('DeleteSelectedMovies')
          : translate('DeleteSelectedMovie')}
      </ModalHeader>

      <ModalBody>
        <div>
          <FormGroup>
            <FormLabel>{translate('AddListExclusion')}</FormLabel>

            <FormInputGroup
              type={inputTypes.CHECK}
              name="addImportExclusion"
              value={addImportExclusion}
              helpText={translate('AddListExclusionMovieHelpText')}
              onChange={onDeleteOptionChange}
            />
          </FormGroup>

          <FormGroup>
            <FormLabel>
              {movies.length > 1
                ? translate('DeleteMovieFolders')
                : translate('DeleteMovieFolder')}
            </FormLabel>

            <FormInputGroup
              type={inputTypes.CHECK}
              name="deleteFiles"
              value={deleteFiles}
              helpText={
                movies.length > 1
                  ? translate('DeleteMovieFoldersHelpText')
                  : translate('DeleteMovieFolderHelpText')
              }
              kind={kinds.DANGER}
              onChange={onDeleteFilesChange}
            />
          </FormGroup>
        </div>

        <div className={styles.message}>
          {deleteFiles
            ? translate('DeleteMovieFolderCountWithFilesConfirmation', {
                count: movies.length,
              })
            : translate('DeleteMovieFolderCountConfirmation', {
                count: movies.length,
              })}
        </div>

        <ul>
          {movies.map(({ title, path, statistics = {} }) => {
            const { movieFileCount = 0, sizeOnDisk = 0 } = statistics;

            return (
              <li key={title}>
                <span>{title}</span>

                {deleteFiles && (
                  <span>
                    <span className={styles.pathContainer}>
                      -<span className={styles.path}>{path}</span>
                    </span>

                    {!!movieFileCount && (
                      <span className={styles.statistics}>
                        (
                        {translate('DeleteMovieFolderMovieCount', {
                          movieFileCount,
                          size: formatBytes(sizeOnDisk),
                        })}
                        )
                      </span>
                    )}
                  </span>
                )}
              </li>
            );
          })}
        </ul>

        {deleteFiles && !!totalMovieFileCount ? (
          <div className={styles.deleteFilesMessage}>
            {translate('DeleteMovieFolderMovieCount', {
              movieFileCount: totalMovieFileCount,
              size: formatBytes(totalSizeOnDisk),
            })}
          </div>
        ) : null}
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
