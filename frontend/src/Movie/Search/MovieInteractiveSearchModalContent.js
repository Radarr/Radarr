import PropTypes from 'prop-types';
import React from 'react';
import Button from 'Components/Link/Button';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import { scrollDirections } from 'Helpers/Props';
import InteractiveSearchConnector from 'InteractiveSearch/InteractiveSearchConnector';
import translate from 'Utilities/String/translate';

function MovieInteractiveSearchModalContent(props) {
  const {
    movieId,
    movieTitle,
    onModalClose
  } = props;

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>
        {movieTitle === undefined ?
          translate('InteractiveSearchModalHeader') :
          translate('InteractiveSearchModalHeaderTitle', { title: movieTitle })
        }
      </ModalHeader>

      <ModalBody scrollDirection={scrollDirections.BOTH}>
        <InteractiveSearchConnector
          searchPayload={{ movieId }}
        />
      </ModalBody>

      <ModalFooter>
        <Button onPress={onModalClose}>
          {translate('Close')}
        </Button>
      </ModalFooter>
    </ModalContent>
  );
}

MovieInteractiveSearchModalContent.propTypes = {
  movieId: PropTypes.number.isRequired,
  movieTitle: PropTypes.string,
  onModalClose: PropTypes.func.isRequired
};

export default MovieInteractiveSearchModalContent;
