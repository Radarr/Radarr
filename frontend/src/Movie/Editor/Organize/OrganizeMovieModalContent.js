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
        Organize Selected Movies
      </ModalHeader>

      <ModalBody>
        <Alert>
          Tip: To preview a rename... select "Cancel" then click any movie title and use the
          <Icon
            className={styles.renameIcon}
            name={icons.ORGANIZE}
          />
        </Alert>

        <div className={styles.message}>
          Are you sure you want to organize all files in the {movieTitles.length} selected movie(s)?
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
          Organize
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
