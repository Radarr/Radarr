import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import Autosuggest from 'react-autosuggest';
import { icons } from 'Helpers/Props';
import Icon from 'Components/Icon';
import keyboardShortcuts, { shortcuts } from 'Components/keyboardShortcuts';
import ArtistSearchResult from './ArtistSearchResult';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import FuseWorker from './fuse.worker';
import styles from './ArtistSearchInput.css';

const LOADING_TYPE = 'suggestionsLoading';
const ADD_NEW_TYPE = 'addNew';
const workerInstance = new FuseWorker();

class ArtistSearchInput extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this._autosuggest = null;

    this.state = {
      value: '',
      suggestions: []
    };
  }

  componentDidMount() {
    this.props.bindShortcut(shortcuts.ARTIST_SEARCH_INPUT.key, this.focusInput);
    workerInstance.addEventListener('message', this.onSuggestionsReceived, false);
  }

  //
  // Control

  setAutosuggestRef = (ref) => {
    this._autosuggest = ref;
  }

  focusInput = (event) => {
    event.preventDefault();
    this._autosuggest.input.focus();
  }

  getSectionSuggestions(section) {
    return section.suggestions;
  }

  renderSectionTitle(section) {
    return (
      <div className={styles.sectionTitle}>
        {section.title}
      </div>
    );
  }

  getSuggestionValue({ title }) {
    return title;
  }

  renderSuggestion(item, { query }) {
    if (item.type === ADD_NEW_TYPE) {
      return (
        <div className={styles.addNewArtistSuggestion}>
          Search for {query}
        </div>
      );
    }

    if (item.type === LOADING_TYPE) {
      return (
        <LoadingIndicator />
      );
    }

    return (
      <ArtistSearchResult
        {...item.item}
        match={item.matches[0]}
      />
    );
  }

  goToArtist(item) {
    this.setState({ value: '' });
    this.props.onGoToArtist(item.item.titleSlug);
  }

  reset() {
    this.setState({
      value: '',
      suggestions: []
    });
  }

  //
  // Listeners

  onChange = (event, { newValue, method }) => {
    if (method === 'up' || method === 'down') {
      return;
    }

    this.setState({ value: newValue });
  }

  onKeyDown = (event) => {
    if (event.key !== 'Tab' && event.key !== 'Enter') {
      return;
    }

    const {
      suggestions,
      value
    } = this.state;

    const {
      highlightedSectionIndex,
      highlightedSuggestionIndex
    } = this._autosuggest.state;

    if (!suggestions.length || suggestions[0].type === LOADING_TYPE || highlightedSectionIndex) {
      this.props.onGoToAddNewArtist(value);
      this._autosuggest.input.blur();
      this.reset();

      return;
    }

    // If an suggestion is not selected go to the first artist,
    // otherwise go to the selected artist.

    if (highlightedSuggestionIndex == null) {
      this.goToArtist(suggestions[0]);
    } else {
      this.goToArtist(suggestions[highlightedSuggestionIndex]);
    }

    this._autosuggest.input.blur();
    this.reset();
  }

  onBlur = () => {
    this.reset();
  }

  onSuggestionsFetchRequested = ({ value }) => {
    this.setState({
      suggestions: [
        {
          type: LOADING_TYPE,
          title: value
        }
      ]
    });
    this.requestSuggestions(value);
  };

  requestSuggestions = _.debounce((value) => {
    const payload = {
      value,
      artists: this.props.artists
    };

    workerInstance.postMessage(payload);
  }, 250);

  onSuggestionsReceived = (message) => {
    this.setState({
      suggestions: message.data
    });
  }

  onSuggestionsClearRequested = () => {
    this.setState({
      suggestions: []
    });
  }

  onSuggestionSelected = (event, { suggestion }) => {
    if (suggestion.type === ADD_NEW_TYPE) {
      this.props.onGoToAddNewArtist(this.state.value);
    } else {
      this.goToArtist(suggestion);
    }
  }

  //
  // Render

  render() {
    const {
      value,
      suggestions
    } = this.state;

    const suggestionGroups = [];

    if (suggestions.length) {
      suggestionGroups.push({
        title: 'Existing Artist',
        suggestions
      });
    }

    suggestionGroups.push({
      title: 'Add New Item',
      suggestions: [
        {
          type: ADD_NEW_TYPE,
          title: value
        }
      ]
    });

    const inputProps = {
      ref: this.setInputRef,
      className: styles.input,
      name: 'artistSearch',
      value,
      placeholder: 'Search',
      autoComplete: 'off',
      spellCheck: false,
      onChange: this.onChange,
      onKeyDown: this.onKeyDown,
      onBlur: this.onBlur,
      onFocus: this.onFocus
    };

    const theme = {
      container: styles.container,
      containerOpen: styles.containerOpen,
      suggestionsContainer: styles.artistContainer,
      suggestionsList: styles.list,
      suggestion: styles.listItem,
      suggestionHighlighted: styles.highlighted
    };

    return (
      <div className={styles.wrapper}>
        <Icon name={icons.SEARCH} />

        <Autosuggest
          ref={this.setAutosuggestRef}
          id={name}
          inputProps={inputProps}
          theme={theme}
          focusInputOnSuggestionClick={false}
          multiSection={true}
          suggestions={suggestionGroups}
          getSectionSuggestions={this.getSectionSuggestions}
          renderSectionTitle={this.renderSectionTitle}
          getSuggestionValue={this.getSuggestionValue}
          renderSuggestion={this.renderSuggestion}
          onSuggestionSelected={this.onSuggestionSelected}
          onSuggestionsFetchRequested={this.onSuggestionsFetchRequested}
          onSuggestionsClearRequested={this.onSuggestionsClearRequested}
        />
      </div>
    );
  }
}

ArtistSearchInput.propTypes = {
  artists: PropTypes.arrayOf(PropTypes.object).isRequired,
  onGoToArtist: PropTypes.func.isRequired,
  onGoToAddNewArtist: PropTypes.func.isRequired,
  bindShortcut: PropTypes.func.isRequired
};

export default keyboardShortcuts(ArtistSearchInput);
