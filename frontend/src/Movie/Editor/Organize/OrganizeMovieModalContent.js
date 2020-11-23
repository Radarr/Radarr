import PropTypes from 'prop-types';
import React from 'react';
import Alert from 'Components/Alert';
import Icon from 'Components/Icon';
import Button from 'Components/Link/Button';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import { icons, kinds } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import styles from './OrganizeMovieModalContent.css';

function OrganizeMovieModalContent(props) {
  const {
    movieTitles,
    onModalClose,
    onOrganizeMoviePress
  } = props;

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>
        {translate('OrganizeSelectedMovies')}
      </ModalHeader>

      <ModalBody>
        <Alert>
          {translate('PreviewRenameHelpText')}
          <Icon
            className={styles.renameIcon}
            name={icons.ORGANIZE}
          />
        </Alert>

        <div className={styles.message}>
          {translate('OrganizeConfirm', [movieTitles.length])}
        </div>

        <ul>
          {
            movieTitles.map((title) => {
              return (
                <li key={title}>
                  {title}
                </li>
              );
            })
          }
        </ul>
      </ModalBody>

      <ModalFooter>
        <Button onPress={onModalClose}>
          {translate('Cancel')}
        </Button>

        <Button
          kind={kinds.DANGER}
          onPress={onOrganizeMoviePress}
        >
          {translate('Organize')}
        </Button>
      </ModalFooter>
    </ModalContent>
  );
}

OrganizeMovieModalContent.propTypes = {
  movieTitles: PropTypes.arrayOf(PropTypes.string).isRequired,
  onModalClose: PropTypes.func.isRequired,
  onOrganizeMoviePress: PropTypes.func.isRequired
};

export default OrganizeMovieModalContent;
