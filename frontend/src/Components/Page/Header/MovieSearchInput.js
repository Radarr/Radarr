import PropTypes from 'prop-types';
import React, { Component } from 'react';
import Autosuggest from 'react-autosuggest';
import jdu from 'jdu';
import { icons } from 'Helpers/Props';
import Icon from 'Components/Icon';
import keyboardShortcuts, { shortcuts } from 'Components/keyboardShortcuts';
import MovieSearchResult from './MovieSearchResult';
import styles from './MovieSearchInput.css';

const ADD_NEW_TYPE = 'addNew';

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
        <div className={styles.addNewSeriesSuggestion}>
          Search for {query}
        </div>
      );
    }

    return (
      <MovieSearchResult
        query={query}
        cleanQuery={jdu.replace(query).toLowerCase()}
        {...item}
      />
    );
  }

  goToMovie(movie) {
    this.setState({ value: '' });
    this.props.onGoToMovie(movie.titleSlug);
  }

  reset() {
    this.setState({
      value: '',
      suggestions: []
    });
  }

  //
  // Listeners

  onChange = (event, { newValue }) => {
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

      return;
    }

    // If an suggestion is not selected go to the first series,
    // otherwise go to the selected series.

    if (highlightedSuggestionIndex == null) {
      this.goToMovie(suggestions[0]);
    } else {
      this.goToMovie(suggestions[highlightedSuggestionIndex]);
    }
  }

  onBlur = () => {
    this.reset();
  }

  onSuggestionsFetchRequested = ({ value }) => {
    const lowerCaseValue = jdu.replace(value).toLowerCase();

    const suggestions = this.props.movie.filter((movie) => {
      // Check the title first and if there isn't a match fallback to
      // the alternate titles and finally the tags.

      return (
        movie.cleanTitle.contains(lowerCaseValue) ||
        movie.alternateTitles.some((alternateTitle) => alternateTitle.cleanTitle.contains(lowerCaseValue)) ||
        movie.tags.some((tag) => tag.cleanLabel.contains(lowerCaseValue))
      );
    });

    this.setState({ suggestions });
  }

  onSuggestionsClearRequested = () => {
    this.reset();
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
      name: 'seriesSearch',
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
  movie: PropTypes.arrayOf(PropTypes.object).isRequired,
  onGoToMovie: PropTypes.func.isRequired,
  onGoToAddNewMovie: PropTypes.func.isRequired,
  bindShortcut: PropTypes.func.isRequired
};

export default keyboardShortcuts(MovieSearchInput);
