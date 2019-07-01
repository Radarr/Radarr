import PropTypes from 'prop-types';
import React, { Component } from 'react';
import Autosuggest from 'react-autosuggest';
import Fuse from 'fuse.js';
import { icons } from 'Helpers/Props';
import Icon from 'Components/Icon';
import keyboardShortcuts, { shortcuts } from 'Components/keyboardShortcuts';
import MovieSearchResult from './MovieSearchResult';
import styles from './MovieSearchInput.css';

const ADD_NEW_TYPE = 'addNew';

const fuseOptions = {
  shouldSort: true,
  includeMatches: true,
  threshold: 0.3,
  location: 0,
  distance: 100,
  maxPatternLength: 32,
  minMatchCharLength: 1,
  keys: [
    'title',
    'alternateTitles.title',
    'tags.label'
  ]
};

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

    if (!suggestions.length || highlightedSectionIndex) {
      this.props.onGoToAddNewMovie(value);
      this._autosuggest.input.blur();
      this.reset();

      return;
    }

    // If an suggestion is not selected go to the first series,
    // otherwise go to the selected series.

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
    const { movies } = this.props;
    let suggestions = [];

    if (value.length === 1) {
      suggestions = movies.reduce((acc, s) => {
        if (s.firstCharacter === value.toLowerCase()) {
          acc.push({
            item: s,
            indices: [
              [0, 0]
            ],
            matches: [
              {
                value: s.title,
                key: 'title'
              }
            ],
            arrayIndex: 0
          });
        }

        return acc;
      }, []);
    } else {
      const fuse = new Fuse(movies, fuseOptions);
      suggestions = fuse.search(value);
    }

    this.setState({ suggestions });
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
