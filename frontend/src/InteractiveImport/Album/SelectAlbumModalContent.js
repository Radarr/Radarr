import PropTypes from 'prop-types';
import React, { Component } from 'react';
import Button from 'Components/Link/Button';
import ModalContent from 'Components/Modal/ModalContent';
import ModalHeader from 'Components/Modal/ModalHeader';
import ModalBody from 'Components/Modal/ModalBody';
import ModalFooter from 'Components/Modal/ModalFooter';
import SelectAlbumRow from './SelectAlbumRow';

class SelectAlbumModalContent extends Component {

  //
  // Render

  render() {
    const {
      items,
      onAlbumSelect,
      onModalClose
    } = this.props;

    return (
      <ModalContent onModalClose={onModalClose}>
        <ModalHeader>
          Manual Import - Select Album
        </ModalHeader>

        <ModalBody>
          {
            items.map((item) => {
              return (
                <SelectAlbumRow
                  key={item.id}
                  id={item.id}
                  title={item.title}
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
  onAlbumSelect: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default SelectAlbumModalContent;
