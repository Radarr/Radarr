import PropTypes from 'prop-types';
import React, { Component } from 'react';
import Button from 'Components/Link/Button';
import { scrollDirections } from 'Helpers/Props';
import ModalContent from 'Components/Modal/ModalContent';
import ModalHeader from 'Components/Modal/ModalHeader';
import ModalBody from 'Components/Modal/ModalBody';
import ModalFooter from 'Components/Modal/ModalFooter';
import Table from 'Components/Table/Table';
import TableBody from 'Components/Table/TableBody';
import SelectEditionRow from './SelectEditionRow';
import Alert from 'Components/Alert';
import styles from './SelectEditionModalContent.css';

const columns = [
  {
    name: 'book',
    label: 'Book',
    isVisible: true
  },
  {
    name: 'edition',
    label: 'Edition',
    isVisible: true
  }
];

class SelectEditionModalContent extends Component {

  //
  // Render

  render() {
    const {
      books,
      onEditionSelect,
      onModalClose,
      ...otherProps
    } = this.props;

    return (
      <ModalContent onModalClose={onModalClose}>
        <ModalHeader>
          Manual Import - Select Edition
        </ModalHeader>

        <ModalBody
          className={styles.modalBody}
          scrollDirection={scrollDirections.NONE}
        >
          <Alert>
            Overrriding an edition here will <b>disable automatic edition selection</b> for that book in future.
          </Alert>

          <Table
            columns={columns}
            {...otherProps}
          >
            <TableBody>
              {
                books.map((item) => {
                  return (
                    <SelectEditionRow
                      key={item.book.id}
                      matchedEditionId={item.matchedEditionId}
                      columns={columns}
                      onEditionSelect={onEditionSelect}
                      {...item.book}
                    />
                  );
                })
              }
            </TableBody>
          </Table>
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

SelectEditionModalContent.propTypes = {
  books: PropTypes.arrayOf(PropTypes.object).isRequired,
  onEditionSelect: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default SelectEditionModalContent;
