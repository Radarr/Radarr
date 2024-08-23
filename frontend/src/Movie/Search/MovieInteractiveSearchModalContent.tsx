import React, { useEffect } from 'react';
import { useDispatch } from 'react-redux';
import Button from 'Components/Link/Button';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import { scrollDirections } from 'Helpers/Props';
import InteractiveSearchConnector from 'InteractiveSearch/InteractiveSearchConnector';
import {
  cancelFetchReleases,
  clearReleases,
} from 'Store/Actions/releaseActions';
import translate from 'Utilities/String/translate';

interface MovieInteractiveSearchModalContentProps {
  movieId: number;
  movieTitle?: string;
  onModalClose(): void;
}

function MovieInteractiveSearchModalContent(
  props: MovieInteractiveSearchModalContentProps
) {
  const { movieId, movieTitle, onModalClose } = props;

  const dispatch = useDispatch();

  useEffect(() => {
    return () => {
      dispatch(cancelFetchReleases());
      dispatch(clearReleases());
    };
  }, [dispatch]);

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>
        {movieTitle
          ? translate('InteractiveSearchModalHeaderTitle', {
              title: movieTitle,
            })
          : translate('InteractiveSearchModalHeader')}
      </ModalHeader>

      <ModalBody scrollDirection={scrollDirections.BOTH}>
        <InteractiveSearchConnector searchPayload={{ movieId }} />
      </ModalBody>

      <ModalFooter>
        <Button onPress={onModalClose}>{translate('Close')}</Button>
      </ModalFooter>
    </ModalContent>
  );
}

export default MovieInteractiveSearchModalContent;
