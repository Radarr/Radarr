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
import Scroller from 'Components/Scroller/Scroller';
import TextInput from 'Components/Form/TextInput';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import SelectAlbumRow from './SelectAlbumRow';
import styles from './SelectAlbumModalContent.css';

const columns = [
  {
    name: 'title',
    label: 'Album Title',
    isVisible: true
  },
  {
    name: 'albumType',
    label: 'Album Type',
    isVisible: true
  },
  {
    name: 'releaseDate',
    label: 'Release Date',
    isVisible: true
  },
  {
    name: 'status',
    label: 'Album Status',
    isVisible: true
  }
];

class SelectAlbumModalContent extends Component {

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
      onAlbumSelect,
      onModalClose,
      isFetching,
      ...otherProps
    } = this.props;

    const filter = this.state.filter;

    return (
      <ModalContent onModalClose={onModalClose}>
        <ModalHeader>
          Manual Import - Select Album
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
            placeholder="Filter album"
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
                          <SelectAlbumRow
                            key={item.id}
                            columns={columns}
                            onAlbumSelect={onAlbumSelect}
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

SelectAlbumModalContent.propTypes = {
  items: PropTypes.arrayOf(PropTypes.object).isRequired,
  isFetching: PropTypes.bool.isRequired,
  onAlbumSelect: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default SelectAlbumModalContent;
