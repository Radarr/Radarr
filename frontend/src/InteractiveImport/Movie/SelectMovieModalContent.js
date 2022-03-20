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

class SelectMovieModalContent extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this._worker = null;

    this.state = {
      scroller: null,
      filter: '',
      loading: false,
      suggestions: props.items
    };
  }

  componentWillUnmount() {
    if (this._worker) {
      this._worker.removeEventListener('message', this.onSuggestionsReceived, false);
      this._worker.terminate();
      this._worker = null;
    }
  }

  getWorker() {
    if (!this._worker) {
      this._worker = new FuseWorker();
      this._worker.addEventListener('message', this.onSuggestionsReceived, false);
    }

    return this._worker;
  }

  //
  // Control

  setScrollerRef = (ref) => {
    this.setState({ scroller: ref });
  };

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
  };

  //
  // Listeners

  onFilterChange = ({ value }) => {
    if (value) {
      this.setState({
        loading: true,
        filter: value.toLowerCase()
      });
      this.requestSuggestions(value);
    } else {
      this.setState({
        loading: false,
        filter: '',
        suggestions: this.props.items
      });
      this.requestSuggestions.cancel();
    }
  };

  requestSuggestions = _.debounce((value) => {
    if (!this.state.loading) {
      return;
    }

    const requestLoading = this.state.requestLoading;

    this.setState({
      requestValue: value,
      requestLoading: true
    });

    if (!requestLoading) {
      const payload = {
        value,
        movies: this.props.items
      };

      this.getWorker().postMessage(payload);
    }
  }, 250);

  onSuggestionsReceived = (message) => {
    const {
      value,
      suggestions
    } = message.data;

    if (!this.state.loading) {
      this.setState({
        requestValue: null,
        requestLoading: false
      });
    } else if (value === this.state.requestValue) {
      this.setState({
        suggestions: suggestions.map((suggestion) => suggestion.item),
        requestValue: null,
        requestLoading: false,
        loading: false
      });
    } else {
      this.setState({
        suggestions: suggestions.map((suggestion) => suggestion.item),
        requestLoading: true
      });

      const payload = {
        value: this.state.requestValue,
        movies: this.props.items
      };

      this.getWorker().postMessage(payload);
    }
  };

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
      loading,
      suggestions
    } = this.state;

    return (
      <ModalContent onModalClose={onModalClose}>
        <ModalHeader>
          <div className={styles.header}>
            {translate('ManualImportSelectMovie')}
          </div>
        </ModalHeader>

        <ModalBody
          className={styles.modalBody}
          scrollDirection={scrollDirections.NONE}
        >
          <TextInput
            className={styles.filterInput}
            placeholder={translate('FilterPlaceHolder')}
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
                loading || !scroller ?
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
