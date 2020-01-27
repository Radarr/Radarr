import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import Autosuggest from 'react-autosuggest';
import { icons } from 'Helpers/Props';
import Icon from 'Components/Icon';
import keyboardShortcuts, { shortcuts } from 'Components/keyboardShortcuts';
import MovieSearchResult from './MovieSearchResult';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import FuseWorker from './fuse.worker';
import styles from './MovieSearchInput.css';

const LOADING_TYPE = 'suggestionsLoading';
const ADD_NEW_TYPE = 'addNew';
const workerInstance = new FuseWorker();

class MovieSearchInput extends Component {

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
    this.props.bindShortcut(shortcuts.MOVIE_SEARCH_INPUT.key, this.focusInput);
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
        <div className={styles.addNewMovieSuggestion}>
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
      <MovieSearchResult
        {...item.item}
        match={item.matches[0]}
      />
    );
  }

  goToMovie(item) {
    this.setState({ value: '' });
    this.props.onGoToMovie(item.item.titleSlug);
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
      this.props.onGoToAddNewMovie(value);
      this._autosuggest.input.blur();
      this.reset();

      return;
    }

    // If an suggestion is not selected go to the first movie,
    // otherwise go to the selected movie.

    if (highlightedSuggestionIndex == null) {
      this.goToMovie(suggestions[0]);
    } else {
      this.goToMovie(suggestions[highlightedSuggestionIndex]);
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
      movies: this.props.movies
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
      this.props.onGoToAddNewMovie(this.state.value);
    } else {
      this.goToMovie(suggestion);
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
        title: 'Existing Movie',
        suggestions
      });
    }

    suggestionGroups.push({
      title: 'Add New Movie',
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
      name: 'movieSearch',
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
      suggestionsContainer: styles.movieContainer,
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

MovieSearchInput.propTypes = {
  movies: PropTypes.arrayOf(PropTypes.object).isRequired,
  onGoToMovie: PropTypes.func.isRequired,
  onGoToAddNewMovie: PropTypes.func.isRequired,
  bindShortcut: PropTypes.func.isRequired
};

export default keyboardShortcuts(MovieSearchInput);
