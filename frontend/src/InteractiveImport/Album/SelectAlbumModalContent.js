import PropTypes from 'prop-types';
import React, { Component } from 'react';
import Button from 'Components/Link/Button';
import ModalContent from 'Components/Modal/ModalContent';
import ModalHeader from 'Components/Modal/ModalHeader';
import ModalBody from 'Components/Modal/ModalBody';
import ModalFooter from 'Components/Modal/ModalFooter';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import SelectAlbumRow from './SelectAlbumRow';

class SelectAlbumModalContent extends Component {

  //
  // Render

  render() {
    const {
      items,
      onAlbumSelect,
      onModalClose,
      isFetching
    } = this.props;

    return (
      <ModalContent onModalClose={onModalClose}>
        <ModalHeader>
          Manual Import - Select Album
        </ModalHeader>

        <ModalBody>
          {
            isFetching &&
              <LoadingIndicator />
          }
          {
            items.map((item) => {
              return (
                <SelectAlbumRow
                  key={item.id}
                  id={item.id}
                  title={item.title}
                  albumType={item.albumType}
                  onAlbumSelect={onAlbumSelect}
                />
              );
            })
          }
        </ModalBody>

        <ModalFooter>
          <Button onPress={onModalClose}>
            Cancel
          </Button>
        </ModalFooter>
      </ModalContent>
    );
  }
}

SelectAlbumModalContent.propTypes = {
  items: PropTypes.arrayOf(PropTypes.object).isRequired,
  isFetching: PropTypes.bool.isRequired,
  onAlbumSelect: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default SelectAlbumModalContent;
