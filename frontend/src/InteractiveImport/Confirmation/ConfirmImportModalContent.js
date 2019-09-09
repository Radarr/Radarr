import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import Button from 'Components/Link/Button';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import { kinds } from 'Helpers/Props';
import ModalContent from 'Components/Modal/ModalContent';
import ModalHeader from 'Components/Modal/ModalHeader';
import ModalBody from 'Components/Modal/ModalBody';
import ModalFooter from 'Components/Modal/ModalFooter';
import Alert from 'Components/Alert';

function formatAlbumFiles(items, album) {

  return (
    <div key={album.id}>
      <b> {album.title} </b>
      <ul>
        {
          _.sortBy(items, 'path').map((item) => {
            return (
              <li key={item.id}>
                {item.path}
              </li>
            );
          })
        }
      </ul>
    </div>
  );

}

class ConfirmImportModalContent extends Component {

  //
  // Lifecycle

  componentDidUpdate(prevProps) {
    const {
      items,
      isFetching,
      isPopulated
    } = this.props;

    if (!isFetching && isPopulated && !items.length) {
      this.props.onModalClose();
      this.props.onConfirmImportPress();
    }
  }

  //
  // Render

  render() {
    const {
      albums,
      items,
      onConfirmImportPress,
      onModalClose,
      isFetching,
      isPopulated
    } = this.props;

    // don't render if nothing to do
    if (!isFetching && isPopulated && !items.length) {
      return null;
    }

    return (
      <ModalContent onModalClose={onModalClose}>

        {
          !isFetching && isPopulated &&
          <ModalHeader>
            Are you sure?
          </ModalHeader>
        }

        <ModalBody>
          {
            isFetching &&
              <LoadingIndicator />
          }

          {
            !isFetching && isPopulated &&
              <div>
                <Alert>
                  You already have files imported for the albums listed below.  If you continue, the existing files <b>will be deleted</b> and the new files imported in their place.

                  To avoid deleting existing files, press 'Cancel' and use the 'Combine with existing files' option.
                </Alert>

                { _.chain(items)
                  .groupBy('albumId')
                  .mapValues((value, key) => formatAlbumFiles(value, _.find(albums, (a) => a.id === parseInt(key))))
                  .values()
                  .value() }
              </div>
          }
        </ModalBody>

        {
          !isFetching && isPopulated &&
            <ModalFooter>
              <Button onPress={onModalClose}>
                Cancel
              </Button>

              <Button
                kind={kinds.DANGER}
                onPress={onConfirmImportPress}
              >
                Proceed
              </Button>

            </ModalFooter>
        }

      </ModalContent>
    );
  }
}

ConfirmImportModalContent.propTypes = {
  albums: PropTypes.arrayOf(PropTypes.object).isRequired,
  items: PropTypes.arrayOf(PropTypes.object).isRequired,
  isFetching: PropTypes.bool.isRequired,
  isPopulated: PropTypes.bool.isRequired,
  onConfirmImportPress: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default ConfirmImportModalContent;
