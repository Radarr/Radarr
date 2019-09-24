import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { scrollDirections } from 'Helpers/Props';
import Button from 'Components/Link/Button';
import Scroller from 'Components/Scroller/Scroller';
import TextInput from 'Components/Form/TextInput';
import ModalContent from 'Components/Modal/ModalContent';
import ModalHeader from 'Components/Modal/ModalHeader';
import ModalBody from 'Components/Modal/ModalBody';
import ModalFooter from 'Components/Modal/ModalFooter';
import SelectMovieRow from './SelectMovieRow';
import styles from './SelectMovieModalContent.css';

class SelectMovieModalContent extends Component {

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
      onMovieSelect,
      onModalClose
    } = this.props;

    const filter = this.state.filter;

    return (
      <ModalContent onModalClose={onModalClose}>
        <ModalHeader>
          Manual Import - Select Movie
        </ModalHeader>

        <ModalBody
          className={styles.modalBody}
          scrollDirection={scrollDirections.NONE}
        >
          <TextInput
            className={styles.filterInput}
            placeholder="Filter movie"
            name="filter"
            value={filter}
            autoFocus={true}
            onChange={this.onFilterChange}
          />

          <Scroller className={styles.scroller}>
            {
              items.map((item) => {
                return item.title.toLowerCase().includes(filter) ?
                  (
                    <SelectMovieRow
                      key={item.id}
                      id={item.id}
                      title={item.title}
                      year={item.year}
                      onMovieSelect={onMovieSelect}
                    />
                  ) :
                  null;
              })
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

SelectMovieModalContent.propTypes = {
  items: PropTypes.arrayOf(PropTypes.object).isRequired,
  onMovieSelect: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default SelectMovieModalContent;
