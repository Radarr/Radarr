import PropTypes from 'prop-types';
import React, { Component } from 'react';
import TextInput from 'Components/Form/TextInput';
import Button from 'Components/Link/Button';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import Scroller from 'Components/Scroller/Scroller';
import Table from 'Components/Table/Table';
import TableBody from 'Components/Table/TableBody';
import { scrollDirections } from 'Helpers/Props';
import SelectBookRow from './SelectBookRow';
import styles from './SelectBookModalContent.css';

const columns = [
  {
    name: 'title',
    label: 'Book Title',
    isSortable: true,
    isVisible: true
  },
  {
    name: 'releaseDate',
    label: 'Release Date',
    isSortable: true,
    isVisible: true
  },
  {
    name: 'status',
    label: 'Book Status',
    isVisible: true
  }
];

class SelectBookModalContent extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      filter: ''
    };
  }

  //
  // Listeners

  onFilterChange = ({ value }) => {
    this.setState({ filter: value.toLowerCase() });
  }

  //
  // Render

  render() {
    const {
      items,
      onBookSelect,
      onModalClose,
      isFetching,
      ...otherProps
    } = this.props;

    const filter = this.state.filter;

    return (
      <ModalContent onModalClose={onModalClose}>
        <ModalHeader>
          Manual Import - Select Book
        </ModalHeader>

        <ModalBody
          className={styles.modalBody}
          scrollDirection={scrollDirections.NONE}
        >
          {
            isFetching &&
              <LoadingIndicator />
          }
          <TextInput
            className={styles.filterInput}
            placeholder="Filter book"
            name="filter"
            value={filter}
            autoFocus={true}
            onChange={this.onFilterChange}
          />

          <Scroller className={styles.scroller}>
            {
              <Table
                columns={columns}
                {...otherProps}
              >
                <TableBody>
                  {
                    items.map((item) => {
                      return item.title.toLowerCase().includes(filter) ?
                        (
                          <SelectBookRow
                            key={item.id}
                            columns={columns}
                            onBookSelect={onBookSelect}
                            {...item}
                          />
                        ) :
                        null;
                    })
                  }
                </TableBody>
              </Table>
            }
          </Scroller>
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

SelectBookModalContent.propTypes = {
  items: PropTypes.arrayOf(PropTypes.object).isRequired,
  isFetching: PropTypes.bool.isRequired,
  onBookSelect: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default SelectBookModalContent;
