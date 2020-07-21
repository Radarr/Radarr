import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import TextInput from 'Components/Form/TextInput';
import Button from 'Components/Link/Button';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import FuseWorker from 'Components/Page/Header/fuse.worker';
import Scroller from 'Components/Scroller/Scroller';
import VirtualTable from 'Components/Table/VirtualTable';
import VirtualTableRow from 'Components/Table/VirtualTableRow';
import { scrollDirections } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import SelectMovieRow from './SelectMovieRow';
import styles from './SelectMovieModalContent.css';

const workerInstance = new FuseWorker();

class SelectMovieModalContent extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      scroller: null,
      filter: '',
      showLoading: false,
      suggestions: props.items
    };
  }

  componentDidMount() {
    workerInstance.addEventListener('message', this.onSuggestionsReceived, false);
  }

  //
  // Control

  setScrollerRef = (ref) => {
    this.setState({ scroller: ref });
  }

  rowRenderer = ({ key, rowIndex, style }) => {
    const item = this.state.suggestions[rowIndex];

    return (
      <VirtualTableRow
        key={key}
        style={style}
      >
        <SelectMovieRow
          key={item.id}
          id={item.id}
          title={item.title}
          year={item.year}
          onMovieSelect={this.props.onMovieSelect}
        />
      </VirtualTableRow>
    );
  }

  //
  // Listeners

  onFilterChange = ({ value }) => {
    if (value) {
      this.setState({
        showLoading: true,
        filter: value.toLowerCase()
      });
      this.requestSuggestions(value);
    } else {
      this.setState({
        showLoading: false,
        filter: '',
        suggestions: this.props.items
      });
      this.requestSuggestions.cancel();
    }
  }

  requestSuggestions = _.debounce((value) => {
    const payload = {
      value,
      movies: this.props.items
    };

    workerInstance.postMessage(payload);
  }, 250);

  onSuggestionsReceived = (message) => {
    this.setState((state, props) => {
      // this guards against setting a stale set of suggestions returned
      // after the filter has been cleared
      if (state.filter !== '') {
        return {
          showLoading: false,
          suggestions: message.data.map((suggestion) => suggestion.item)
        };
      }
      return {};
    });
  }

  //
  // Render

  render() {
    const {
      relativePath,
      onModalClose
    } = this.props;

    const {
      scroller,
      filter,
      showLoading,
      suggestions
    } = this.state;

    return (
      <ModalContent onModalClose={onModalClose}>
        <ModalHeader>
          <div className={styles.header}>
            Manual Import - Select Movie
          </div>
        </ModalHeader>

        <ModalBody
          className={styles.modalBody}
          scrollDirection={scrollDirections.NONE}
        >
          <TextInput
            className={styles.filterInput}
            placeholder="Search movies"
            name="filter"
            value={filter}
            autoFocus={true}
            onChange={this.onFilterChange}
          />

          <Scroller
            registerScroller={this.setScrollerRef}
            className={styles.scroller}
            autoFocus={false}
          >
            <div>
              {
                showLoading || !scroller ?
                  <LoadingIndicator /> :
                  <VirtualTable
                    header={
                      <div />
                    }
                    items={suggestions}
                    isSmallScreen={false}
                    scroller={scroller}
                    focusScroller={false}
                    rowRenderer={this.rowRenderer}
                  />
              }
            </div>
          </Scroller>
        </ModalBody>

        <ModalFooter className={styles.footer}>
          <div className={styles.path}>{relativePath}</div>
          <div className={styles.buttons}>
            <Button onPress={onModalClose}>
              {translate('Cancel')}
            </Button>
          </div>
        </ModalFooter>
      </ModalContent>
    );
  }
}

SelectMovieModalContent.propTypes = {
  items: PropTypes.arrayOf(PropTypes.object).isRequired,
  relativePath: PropTypes.string.isRequired,
  onMovieSelect: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default SelectMovieModalContent;
