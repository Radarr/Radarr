import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { kinds } from 'Helpers/Props';
import Button from 'Components/Link/Button';
import ModalContent from 'Components/Modal/ModalContent';
import ModalHeader from 'Components/Modal/ModalHeader';
import ModalBody from 'Components/Modal/ModalBody';
import ModalFooter from 'Components/Modal/ModalFooter';
import styles from './ExcludeMovieModalContent.css';

class ExcludeMovieModalContent extends Component {

  //
  // Listeners

  onExcludeMovieConfirmed = () => {
    this.props.onExcludePress();
  }

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
            Exclude {title}? This will prevent Radarr from adding automatically via list sync.
          </div>

        </ModalBody>

        <ModalFooter>
          <Button onPress={onModalClose}>
            Close
          </Button>

          <Button
            kind={kinds.DANGER}
            onPress={this.onExcludeMovieConfirmed}
          >
            Exlude
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
