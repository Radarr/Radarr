import PropTypes from 'prop-types';
import React, { Component } from 'react';
import Button from 'Components/Link/Button';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import { kinds } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import styles from './ExcludeMovieModalContent.css';

class ExcludeMovieModalContent extends Component {

  //
  // Listeners

  onExcludeMovieConfirmed = () => {
    this.props.onExcludePress();
  };

  //
  // Render

  render() {
    const {
      tmdbId,
      title,
      onModalClose
    } = this.props;

    return (
      <ModalContent
        onModalClose={onModalClose}
      >
        <ModalHeader>
          Exclude - {title} ({tmdbId})
        </ModalHeader>

        <ModalBody>
          <div className={styles.pathContainer}>
            {translate('ExcludeTitle', [title])}
          </div>

        </ModalBody>

        <ModalFooter>
          <Button onPress={onModalClose}>
            {translate('Close')}
          </Button>

          <Button
            kind={kinds.DANGER}
            onPress={this.onExcludeMovieConfirmed}
          >
            Exclude
          </Button>
        </ModalFooter>
      </ModalContent>
    );
  }
}

ExcludeMovieModalContent.propTypes = {
  tmdbId: PropTypes.number.isRequired,
  title: PropTypes.string.isRequired,
  onExcludePress: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default ExcludeMovieModalContent;
