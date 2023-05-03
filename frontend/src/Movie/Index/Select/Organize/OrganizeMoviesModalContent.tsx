import { orderBy } from 'lodash';
import React, { useCallback, useMemo } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { RENAME_MOVIE } from 'Commands/commandNames';
import Alert from 'Components/Alert';
import Icon from 'Components/Icon';
import Button from 'Components/Link/Button';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import { icons, kinds } from 'Helpers/Props';
import Movie from 'Movie/Movie';
import { executeCommand } from 'Store/Actions/commandActions';
import createAllMoviesSelector from 'Store/Selectors/createAllMoviesSelector';
import translate from 'Utilities/String/translate';
import styles from './OrganizeMoviesModalContent.css';

interface OrganizeMoviesModalContentProps {
  movieIds: number[];
  onModalClose: () => void;
}

function OrganizeMoviesModalContent(props: OrganizeMoviesModalContentProps) {
  const { movieIds, onModalClose } = props;

  const allMovies: Movie[] = useSelector(createAllMoviesSelector());
  const dispatch = useDispatch();

  const movieTitles = useMemo(() => {
    const movie = movieIds.reduce((acc: Movie[], id) => {
      const s = allMovies.find((s) => s.id === id);

      if (s) {
        acc.push(s);
      }

      return acc;
    }, []);

    const sorted = orderBy(movie, ['sortTitle']);

    return sorted.map((s) => s.title);
  }, [movieIds, allMovies]);

  const onOrganizePress = useCallback(() => {
    dispatch(
      executeCommand({
        name: RENAME_MOVIE,
        movieIds,
      })
    );

    onModalClose();
  }, [movieIds, onModalClose, dispatch]);

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>{translate('OrganizeSelectedMovies')}</ModalHeader>

      <ModalBody>
        <Alert>
          {translate('PreviewRenameHelpText')}
          <Icon className={styles.renameIcon} name={icons.ORGANIZE} />
        </Alert>

        <div className={styles.message}>
          {translate('OrganizeConfirm', movieTitles.length)}
        </div>

        <ul>
          {movieTitles.map((title) => {
            return <li key={title}>{title}</li>;
          })}
        </ul>
      </ModalBody>

      <ModalFooter>
        <Button onPress={onModalClose}>{translate('Cancel')}</Button>

        <Button kind={kinds.DANGER} onPress={onOrganizePress}>
          {translate('Organize')}
        </Button>
      </ModalFooter>
    </ModalContent>
  );
}

export default OrganizeMoviesModalContent;
